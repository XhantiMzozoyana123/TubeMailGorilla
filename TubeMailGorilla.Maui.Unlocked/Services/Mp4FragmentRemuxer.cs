using System.Buffers.Binary;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Converts a fragmented MP4 (fMP4 - the layout YouTube serves for its DASH
/// streams) into a plain progressive MP4 by folding the moof/mdat fragments
/// into a single mdat plus real sample tables.
///
/// Why this exists: Windows Media Foundation's file source reads the metadata
/// of a fragmented file fine (duration comes back correct) but cannot serve
/// samples from one - MediaComposition.GetThumbnailAsync fails with
/// "timeFromStart is outside of the composition bounds" for every timestamp and
/// MediaTranscoder reports "no samples were processed by the sink". A
/// progressive MP4 works, and the app promises no ffmpeg dependency, so the
/// fragments are folded here in managed code instead.
///
/// Scope: single-video-track fMP4 as YouTube produces it (tfhd with
/// default-base-is-moof, trun with data_offset/duration/size/flags/cto).
/// Anything unexpected makes <see cref="TryRemux"/> return false and the caller
/// simply keeps the original file - a remux failure must never lose a video.
/// </summary>
public static class Mp4FragmentRemuxer
{
    /// <summary>
    /// Cheap header-only check: does the file contain top-level moof boxes
    /// (the fragmented-MP4 marker)? Avoids reading the whole file just to
    /// discover it needs no remux.
    /// </summary>
    public static bool LooksFragmented(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            Span<byte> header = stackalloc byte[16];
            var remaining = fs.Length;

            while (remaining >= 8)
            {
                if (fs.Read(header[..8]) != 8) return false;

                var size32 = BinaryPrimitives.ReadUInt32BigEndian(header[..4]);
                var type = BinaryPrimitives.ReadUInt32BigEndian(header[4..8]);
                long boxSize = size32;
                var headerSize = 8;

                if (size32 == 1)
                {
                    if (remaining < 16 || fs.Read(header[..8]) != 8) return false;
                    var large = BinaryPrimitives.ReadUInt64BigEndian(header[..8]);
                    if (large > (ulong)remaining) return false;
                    boxSize = (long)large;
                    headerSize = 16;
                }
                else if (size32 == 0)
                {
                    boxSize = remaining;
                }

                if (boxSize < headerSize || boxSize > remaining) return false;

                if (type == 0x6D6F6F66) return true; // moof

                fs.Position += boxSize - headerSize;
                remaining -= boxSize;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads <paramref name="sourcePath"/> and writes a progressive MP4 to
    /// <paramref name="targetPath"/>. Returns true on success; on false the
    /// target file (if any) is deleted and the source is untouched.
    /// </summary>
    public static bool TryRemux(string sourcePath, string targetPath)
    {
        try
        {
            var source = new FileInfo(sourcePath);
            // Snapshots only ever need small files; a runaway size guard keeps a
            // pathological download from ballooning the temp folder.
            if (!source.Exists || source.Length < 64 || source.Length > 1024L * 1024 * 1024)
                return false;

            var data = File.ReadAllBytes(sourcePath);
            if (!TryRemuxCore(data, out var output))
                return false;

            File.WriteAllBytes(targetPath, output);
            return true;
        }
        catch
        {
            TryDelete(targetPath);
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    // ------------------------------------------------------------------
    //  Box plumbing
    // ------------------------------------------------------------------

    private readonly record struct Box(int Offset, int Size, uint Type);

    private static uint Type(Box b, byte[] data) =>
        BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(b.Offset + 4, 4));

    /// <summary>Iterates the direct children of a box's payload.</summary>
    private static IEnumerable<Box> Children(byte[] data, int payloadOffset, int payloadLength)
    {
        var pos = payloadOffset;
        var end = payloadOffset + payloadLength;
        while (pos + 8 <= end)
        {
            var size = (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos, 4));
            var type = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos + 4, 4));
            int boxSize;
            if (size == 1)
            {
                if (pos + 16 > end) yield break;
                var large = BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(pos + 8, 8));
                if (large > int.MaxValue || large < 16) yield break;
                boxSize = (int)large;
            }
            else if (size == 0)
            {
                boxSize = end - pos;
            }
            else if (size < 8)
            {
                yield break;
            }
            else
            {
                boxSize = size;
            }

            if (pos + boxSize > end) yield break;

            yield return new Box(pos, boxSize, type);
            pos += boxSize;
        }
    }

    private static Box? FindChild(byte[] data, int payloadOffset, int payloadLength, uint type)
    {
        foreach (var b in Children(data, payloadOffset, payloadLength))
            if (Type(b, data) == type)
                return b;
        return null;
    }

    private static Box? FindPath(byte[] data, Box parent, params uint[] path)
    {
        var current = parent;
        foreach (var type in path)
        {
            var next = FindChild(data, current.Offset + 8, current.Size - 8, type);
            if (next is null) return null;
            current = next.Value;
        }
        return current;
    }

    private static uint FourCc(string s) =>
        BinaryPrimitives.ReadUInt32BigEndian(System.Text.Encoding.ASCII.GetBytes(s));

    // ------------------------------------------------------------------
    //  Core
    // ------------------------------------------------------------------

    private sealed class Sample
    {
        public long Offset;      // absolute position in the SOURCE file
        public uint Size;
        public uint Duration;    // in media (mdhd) timescale
        public int CompositionOffset;
        public bool Sync;
    }

    private static bool TryRemuxCore(byte[] data, out byte[] output)
    {
        output = Array.Empty<byte>();

        // --- top-level scan ---
        Box? ftyp = null, moov = null;
        var moofs = new List<Box>();
        foreach (var box in Children(data, 0, data.Length))
        {
            switch (Type(box, data))
            {
                case 0x66747970: ftyp ??= box; break; // ftyp
                case 0x6D6F6F76: moov ??= box; break; // moov
                case 0x6D6F6F66: moofs.Add(box); break; // moof
            }
        }

        if (ftyp is null || moov is null || moofs.Count == 0)
            return false; // not fragmented (or malformed) - nothing to remux

        // --- locate the video trak ---
        Box? videoTrak = null;
        uint videoTrackId = 0;
        foreach (var trak in Children(data, moov.Value.Offset + 8, moov.Value.Size - 8))
        {
            if (Type(trak, data) != 0x7472616B) continue; // trak

            var hdlr = FindPath(data, trak, 0x6D646961 /*mdia*/, 0x68646C72 /*hdlr*/);
            if (hdlr is null) continue;
            // handler_type sits at payload offset 8 of the hdlr full box.
            var handler = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(hdlr.Value.Offset + 16, 4));
            if (handler != FourCc("vide")) continue;

            var tkhd = FindChild(data, trak.Offset + 8, trak.Size - 8, 0x746B6864 /*tkhd*/);
            if (tkhd is null) continue;

            // tkhd payload: version(1) flags(3) creation(4|8) modification(4|8) trackID(4)
            var version = data[tkhd.Value.Offset + 8];
            var idPos = tkhd.Value.Offset + 8 + (version == 1 ? 4 + 8 + 8 : 4 + 4 + 4);
            videoTrackId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(idPos, 4));
            videoTrak = trak;
            break;
        }

        if (videoTrak is null || videoTrackId == 0)
            return false;

        var stbl = FindPath(data, videoTrak.Value,
            0x6D646961 /*mdia*/, 0x6D696E66 /*minf*/, 0x7374626C /*stbl*/);
        if (stbl is null) return false;

        var mdhd = FindPath(data, videoTrak.Value, 0x6D646961 /*mdia*/, 0x6D646864 /*mdhd*/);
        if (mdhd is null) return false;
        var mdhdVersion = data[mdhd.Value.Offset + 8];
        var timescalePos = mdhd.Value.Offset + 8 + (mdhdVersion == 1 ? 4 + 8 + 8 : 4 + 4 + 4);
        var mediaTimescale = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(timescalePos, 4));
        if (mediaTimescale == 0) return false;

        // --- walk the fragments in file order, collecting samples ---
        var samples = new List<Sample>();
        foreach (var moof in moofs.OrderBy(b => b.Offset))
        {
            if (!TryReadFragment(data, moof, videoTrackId, out var fragmentSamples))
                return false;
            samples.AddRange(fragmentSamples);
        }

        if (samples.Count == 0) return false;

        var totalDuration = samples.Aggregate(0UL, (acc, s) => acc + s.Duration);

        return RebuildMoov(data, moov.Value, stbl.Value, videoTrak.Value, mdhd.Value,
            samples, totalDuration, mediaTimescale, out output);
    }

    /// <summary>
    /// Reads one moof box and turns its traf/trun sample rows into absolute
    /// source-file sample entries. Returns false on anything unexpected.
    /// </summary>
    private static bool TryReadFragment(byte[] data, Box moof, uint videoTrackId, out List<Sample> samples)
    {
        samples = new List<Sample>();

        foreach (var traf in Children(data, moof.Offset + 8, moof.Size - 8))
        {
            if (Type(traf, data) != 0x74726166) continue; // traf

            var tfhd = FindChild(data, traf.Offset + 8, traf.Size - 8, 0x74666864); // tfhd
            if (tfhd is null) return false;

            var tfhdFlags = ReadFlags(data, tfhd.Value.Offset);
            var trackId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(tfhd.Value.Offset + 12, 4));
            if (trackId != videoTrackId) continue;

            var cursor = tfhd.Value.Offset + 16;
            ulong baseDataOffset = 0;
            if ((tfhdFlags & 0x000001) != 0) // base-data-offset present
            {
                baseDataOffset = BinaryPrimitives.ReadUInt64BigEndian(data.AsSpan(cursor, 8));
                cursor += 8;
            }
            if ((tfhdFlags & 0x000002) != 0) cursor += 4; // sample-description-index
            uint? defaultDuration = null, defaultSize = null, defaultFlags = null;
            if ((tfhdFlags & 0x000008) != 0)
            {
                defaultDuration = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(cursor, 4));
                cursor += 4;
            }
            if ((tfhdFlags & 0x000010) != 0)
            {
                defaultSize = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(cursor, 4));
                cursor += 4;
            }
            if ((tfhdFlags & 0x000020) != 0)
            {
                defaultFlags = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(cursor, 4));
                cursor += 4;
            }

            // Default-base-is-moof: chunk bases are relative to the moof itself;
            // otherwise the explicit base-data-offset (0 when absent) is used.
            var defaultBaseIsMoof = (tfhdFlags & 0x020000) != 0;
            var chunkBase = defaultBaseIsMoof ? (ulong)moof.Offset : baseDataOffset;

            // Implicit continuation position for truns without a data_offset.
            long implicitPos = (long)chunkBase;

            foreach (var child in Children(data, traf.Offset + 8, traf.Size - 8))
            {
                if (Type(child, data) == 0x7472756E) // trun
                {
                    if (!TryReadTrun(data, child, chunkBase, defaultDuration, defaultSize,
                            defaultFlags, ref implicitPos, out var trunSamples))
                        return false;
                    samples.AddRange(trunSamples);
                }
                // tfdt and mfhd are not needed: offsets come from the fragment
                // layout and durations from trun/tfhd.
            }
        }

        return samples.Count > 0;
    }

    private static uint ReadFlags(byte[] data, int boxOffset)
    {
        // Full box: version(1) then flags(3) big-endian.
        return (uint)((data[boxOffset + 9] << 16) | (data[boxOffset + 10] << 8) | data[boxOffset + 11]);
    }

    private static bool TryReadTrun(
        byte[] data,
        Box trun,
        ulong chunkBase,
        uint? defaultDuration,
        uint? defaultSize,
        uint? defaultFlags,
        ref long implicitPos,
        out List<Sample> samples)
    {
        samples = new List<Sample>();

        var version = data[trun.Offset + 8];
        var flags = ReadFlags(data, trun.Offset);
        var sampleCount = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(trun.Offset + 12, 4));
        if (sampleCount == 0 || sampleCount > 10_000_000) return false;

        var pos = trun.Offset + 16;
        long chunkStart = implicitPos;
        if ((flags & 0x000001) != 0) // data-offset present
        {
            var dataOffset = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(pos, 4));
            pos += 4;
            chunkStart = (long)chunkBase + dataOffset;
        }

        uint? firstFlags = null;
        if ((flags & 0x000004) != 0) // first-sample-flags present
        {
            firstFlags = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos, 4));
            pos += 4;
        }

        var hasDuration = (flags & 0x000100) != 0;
        var hasSize = (flags & 0x000200) != 0;
        var hasFlags = (flags & 0x000400) != 0;
        var hasCto = (flags & 0x000800) != 0;
        var perSampleSize = (hasDuration ? 4 : 0) + (hasSize ? 4 : 0) + (hasFlags ? 4 : 0) + (hasCto ? 4 : 0);

        if (pos + (long)sampleCount * perSampleSize > trun.Offset + trun.Size)
            return false; // truncated trun

        var offset = chunkStart;
        for (uint i = 0; i < sampleCount; i++)
        {
            uint duration = 0, size = 0, sampleFlags = 0;
            if (hasDuration)
            {
                duration = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos, 4));
                pos += 4;
            }
            if (hasSize)
            {
                size = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos, 4));
                pos += 4;
            }
            if (hasFlags)
            {
                sampleFlags = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos, 4));
                pos += 4;
            }
            var cto = 0;
            if (hasCto)
            {
                cto = version == 1
                    ? BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(pos, 4))
                    : (int)BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos, 4));
                pos += 4;
            }

            duration = hasDuration ? duration : defaultDuration ?? 0;
            size = hasSize ? size : defaultSize ?? 0;
            if (duration == 0 || size == 0) return false; // cannot place the sample

            var flagsValue = hasFlags
                ? sampleFlags
                : (i == 0 ? firstFlags ?? defaultFlags ?? 0 : defaultFlags ?? 0);

            samples.Add(new Sample
            {
                Offset = offset,
                Size = size,
                Duration = duration,
                CompositionOffset = cto,
                // sample_is_non_sync_sample is bit 16; absent flags = keyframe.
                Sync = (flagsValue & 0x00010000) == 0,
            });

            offset += size;
        }

        implicitPos = offset;
        return true;
    }

    // ------------------------------------------------------------------
    //  Sample table builders
    // ------------------------------------------------------------------

    private static byte[] MakeBox(uint type, byte[] payload)
    {
        var box = new byte[8 + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(box.AsSpan(0, 4), (uint)box.Length);
        BinaryPrimitives.WriteUInt32BigEndian(box.AsSpan(4, 4), type);
        payload.CopyTo(box.AsSpan(8));
        return box;
    }

    private static byte[] FullBoxHeader(byte version, uint flags) => new[]
    {
        version,
        (byte)((flags >> 16) & 0xFF),
        (byte)((flags >> 8) & 0xFF),
        (byte)(flags & 0xFF),
    };

    /// <summary>stts - sample durations (run-length encoded).</summary>
    private static byte[] BuildStts(List<Sample> samples)
    {
        var entries = new List<(uint Count, uint Delta)>();
        foreach (var s in samples)
        {
            if (entries.Count > 0 && entries[^1].Delta == s.Duration)
                entries[^1] = (entries[^1].Count + 1, s.Duration);
            else
                entries.Add((1, s.Duration));
        }

        var payload = new byte[4 + 4 + entries.Count * 8];
        FullBoxHeader(0, 0).CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)entries.Count);
        for (var i = 0; i < entries.Count; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8 + i * 8, 4), entries[i].Count);
            BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(12 + i * 8, 4), entries[i].Delta);
        }
        return MakeBox(0x73747473 /*stts*/, payload);
    }

    /// <summary>stsc - one chunk per sample, so a single run covers them all.</summary>
    private static byte[] BuildStsc()
    {
        var payload = new byte[4 + 4 + 12];
        FullBoxHeader(0, 0).CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), 1);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8, 4), 1);           // first_chunk
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(12, 4), 1);          // samples_per_chunk
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(16, 4), 1);          // sample_description_index
        return MakeBox(0x73747363 /*stsc*/, payload);
    }

    /// <summary>stsz - every sample's size.</summary>
    private static byte[] BuildStsz(List<Sample> samples)
    {
        var payload = new byte[4 + 4 + 4 + samples.Count * 4];
        FullBoxHeader(0, 0).CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), 0);            // sample_size = 0 (variable)
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8, 4), (uint)samples.Count);
        for (var i = 0; i < samples.Count; i++)
            BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(12 + i * 4, 4), samples[i].Size);
        return MakeBox(0x7374737A /*stsz*/, payload);
    }

    /// <summary>stco - chunk offsets; values are patched once the layout is known.</summary>
    private static byte[] BuildStco(int chunkCount)
    {
        var payload = new byte[4 + 4 + chunkCount * 4];
        FullBoxHeader(0, 0).CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)chunkCount);
        return MakeBox(0x7374636F /*stco*/, payload);
    }

    /// <summary>ctts - composition offsets; null when every offset is zero.</summary>
    private static byte[]? BuildCtts(List<Sample> samples)
    {
        if (samples.All(s => s.CompositionOffset == 0)) return null;

        var entries = new List<(uint Count, int Offset)>();
        foreach (var s in samples)
        {
            if (entries.Count > 0 && entries[^1].Offset == s.CompositionOffset)
                entries[^1] = (entries[^1].Count + 1, s.CompositionOffset);
            else
                entries.Add((1, s.CompositionOffset));
        }

        var version = samples.Any(s => s.CompositionOffset < 0) ? (byte)1 : (byte)0;
        var payload = new byte[4 + 4 + entries.Count * 8];
        FullBoxHeader(version, 0).CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)entries.Count);
        for (var i = 0; i < entries.Count; i++)
        {
            BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8 + i * 8, 4), entries[i].Count);
            if (version == 1)
                BinaryPrimitives.WriteInt32BigEndian(payload.AsSpan(12 + i * 8, 4), entries[i].Offset);
            else
                BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(12 + i * 8, 4), (uint)entries[i].Offset);
        }
        return MakeBox(0x63747473 /*ctts*/, payload);
    }

    /// <summary>stss - sync sample numbers; null when every sample is a keyframe.</summary>
    private static byte[]? BuildStss(List<Sample> samples)
    {
        var sync = new List<uint>();
        for (var i = 0; i < samples.Count; i++)
            if (samples[i].Sync)
                sync.Add((uint)(i + 1));

        if (sync.Count == samples.Count) return null;

        var payload = new byte[4 + 4 + sync.Count * 4];
        FullBoxHeader(0, 0).CopyTo(payload, 0);
        BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(4, 4), (uint)sync.Count);
        for (var i = 0; i < sync.Count; i++)
            BinaryPrimitives.WriteUInt32BigEndian(payload.AsSpan(8 + i * 4, 4), sync[i]);
        return MakeBox(0x73747373 /*stss*/, payload);
    }

    // ------------------------------------------------------------------
    //  moov rebuild and final assembly
    // ------------------------------------------------------------------

    private static readonly uint[] ReplacedStblTables =
    {
        0x73747473, // stts
        0x73747363, // stsc
        0x7374737A, // stsz
        0x73747A32, // stz2
        0x7374636F, // stco
        0x636F3634, // co64
        0x63747473, // ctts
        0x73747373, // stss
    };

    /// <summary>
    /// Builds the progressive moov from the original one (tables swapped,
    /// durations patched, mvex and non-video traks dropped) and assembles the
    /// final ftyp | moov | mdat file with patched chunk offsets.
    /// </summary>
    private static bool RebuildMoov(
        byte[] data,
        Box moov,
        Box stbl,
        Box videoTrak,
        Box mdhd,
        List<Sample> samples,
        ulong totalDuration,
        uint mediaTimescale,
        out byte[] output)
    {
        output = Array.Empty<byte>();

        Box? ftyp = null;
        foreach (var box in Children(data, 0, data.Length))
        {
            if (Type(box, data) == 0x66747970) { ftyp = box; break; }
        }
        if (ftyp is null) return false;

        var mvhd = FindChild(data, moov.Offset + 8, moov.Size - 8, 0x6D766864 /*mvhd*/);
        if (mvhd is null) return false;
        var mvhdVersion = data[mvhd.Value.Offset + 8];
        var movieTimescalePos = mvhd.Value.Offset + 8 + (mvhdVersion == 1 ? 4 + 8 + 8 : 4 + 4 + 4);
        var movieTimescale = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(movieTimescalePos, 4));
        if (movieTimescale == 0) return false;

        var movieDuration64 = (totalDuration * movieTimescale + mediaTimescale / 2) / mediaTimescale;
        if (movieDuration64 > uint.MaxValue && mvhdVersion == 0) return false;

        // --- rebuild the video trak ---
        var trakChildren = new List<byte[]>();
        foreach (var child in Children(data, videoTrak.Offset + 8, videoTrak.Size - 8))
        {
            var childType = Type(child, data);
            if (childType == 0x746B6864) // tkhd - patch duration (movie timescale)
            {
                var copy = data.AsSpan(child.Offset, child.Size).ToArray();
                if (copy[8] == 1)
                    BinaryPrimitives.WriteUInt64BigEndian(copy.AsSpan(36, 8), movieDuration64);
                else
                    BinaryPrimitives.WriteUInt32BigEndian(copy.AsSpan(28, 4), (uint)movieDuration64);
                trakChildren.Add(copy);
            }
            else if (childType == 0x6D646961) // mdia
            {
                if (!RebuildMdia(data, child, stbl, samples, totalDuration, out var mdiaBytes))
                    return false;
                trakChildren.Add(mdiaBytes);
            }
            else
            {
                trakChildren.Add(data.AsSpan(child.Offset, child.Size).ToArray());
            }
        }
        var trakBytes = MakeBox(0x7472616B /*trak*/, Concat(trakChildren));

        // --- rebuild the whole moov ---
        var moovChildren = new List<byte[]>();
        foreach (var child in Children(data, moov.Offset + 8, moov.Size - 8))
        {
            var childType = Type(child, data);
            if (childType == 0x7472616B) // trak: keep only the video one
            {
                if (child.Offset == videoTrak.Offset) moovChildren.Add(trakBytes);
            }
            else if (childType == 0x6D766578) // mvex: marks the file as fragmented
            {
                // dropped on purpose
            }
            else if (childType == 0x6D766864) // mvhd - patch duration
            {
                var copy = data.AsSpan(child.Offset, child.Size).ToArray();
                if (copy[8] == 1)
                    BinaryPrimitives.WriteUInt64BigEndian(copy.AsSpan(32, 8), movieDuration64);
                else
                    BinaryPrimitives.WriteUInt32BigEndian(copy.AsSpan(24, 4), (uint)movieDuration64);
                moovChildren.Add(copy);
            }
            else
            {
                moovChildren.Add(data.AsSpan(child.Offset, child.Size).ToArray());
            }
        }
        var moovBytes = MakeBox(0x6D6F6F76 /*moov*/, Concat(moovChildren));

        // --- assemble ftyp | moov | mdat and patch chunk offsets ---
        var stcoPayloadIndex = FindStcoPayloadIndex(moovBytes);
        if (stcoPayloadIndex < 0) return false;
        if (stcoPayloadIndex + samples.Count * 4 > moovBytes.Length) return false;

        var ftypBytes = data.AsSpan(ftyp.Value.Offset, ftyp.Value.Size).ToArray();
        var mdatStart = (long)ftypBytes.Length + moovBytes.Length + 8;
        long mdatLength = 0;
        foreach (var s in samples) mdatLength += s.Size;
        if (mdatLength > int.MaxValue) return false;

        var mdatPayload = new byte[mdatLength];
        var cursor = 0L;
        for (var i = 0; i < samples.Count; i++)
        {
            var s = samples[i];
            var srcStart = (int)s.Offset;
            if (srcStart < 0 || (long)srcStart + s.Size > data.Length) return false;
            Buffer.BlockCopy(data, srcStart, mdatPayload, (int)cursor, (int)s.Size);

            BinaryPrimitives.WriteUInt32BigEndian(
                moovBytes.AsSpan(stcoPayloadIndex + i * 4, 4),
                (uint)(mdatStart + cursor));
            cursor += s.Size;
        }

        using var ms = new MemoryStream();
        ms.Write(ftypBytes);
        ms.Write(moovBytes);
        WriteBox(ms, 0x6D646174 /*mdat*/, mdatPayload);

        output = ms.ToArray();
        return output.Length > ftypBytes.Length + moovBytes.Length + 8;
    }

    private static byte[] Concat(List<byte[]> parts)
    {
        var total = parts.Sum(p => p.Length);
        var result = new byte[total];
        var at = 0;
        foreach (var part in parts)
        {
            part.CopyTo(result, at);
            at += part.Length;
        }
        return result;
    }

    /// <summary>mdia: patches the media duration and swaps the stbl tables.</summary>
    private static bool RebuildMdia(
        byte[] data, Box mdia, Box stbl, List<Sample> samples, ulong totalDuration, out byte[] output)
    {
        output = Array.Empty<byte>();
        var children = new List<byte[]>();

        foreach (var child in Children(data, mdia.Offset + 8, mdia.Size - 8))
        {
            var childType = Type(child, data);
            if (childType == 0x6D646864) // mdhd - patch duration (media timescale)
            {
                var copy = data.AsSpan(child.Offset, child.Size).ToArray();
                if (copy[8] == 1)
                    BinaryPrimitives.WriteUInt64BigEndian(copy.AsSpan(32, 8), totalDuration);
                else
                    BinaryPrimitives.WriteUInt32BigEndian(copy.AsSpan(24, 4), (uint)totalDuration);
                children.Add(copy);
            }
            else if (childType == 0x6D696E66) // minf
            {
                if (!RebuildMinf(data, child, stbl, samples, out var minfBytes))
                    return false;
                children.Add(minfBytes);
            }
            else
            {
                children.Add(data.AsSpan(child.Offset, child.Size).ToArray());
            }
        }

        output = MakeBox(0x6D646961 /*mdia*/, Concat(children));
        return true;
    }

    /// <summary>minf: everything kept except the rebuilt stbl.</summary>
    private static bool RebuildMinf(byte[] data, Box minf, Box stbl, List<Sample> samples, out byte[] output)
    {
        output = Array.Empty<byte>();
        var children = new List<byte[]>();

        foreach (var child in Children(data, minf.Offset + 8, minf.Size - 8))
        {
            if (child.Offset == stbl.Offset)
            {
                if (!RebuildStbl(data, stbl, samples, out var stblBytes))
                    return false;
                children.Add(stblBytes);
            }
            else
            {
                children.Add(data.AsSpan(child.Offset, child.Size).ToArray());
            }
        }

        output = MakeBox(0x6D696E66 /*minf*/, Concat(children));
        return true;
    }

    /// <summary>
    /// stbl: the codec description (stsd) and any grouping boxes are kept as
    /// they are; the timing/offset tables are rebuilt from the samples.
    /// </summary>
    private static bool RebuildStbl(byte[] data, Box stbl, List<Sample> samples, out byte[] output)
    {
        output = Array.Empty<byte>();
        var children = new List<byte[]>();
        var sawStsd = false;

        foreach (var child in Children(data, stbl.Offset + 8, stbl.Size - 8))
        {
            var childType = Type(child, data);
            if (childType == 0x73747364) sawStsd = true;

            if (ReplacedStblTables.Contains(childType))
                continue; // rebuilt below

            children.Add(data.AsSpan(child.Offset, child.Size).ToArray());
        }

        if (!sawStsd) return false;

        children.Add(BuildStts(samples));
        var stss = BuildStss(samples);
        if (stss is not null) children.Add(stss);
        var ctts = BuildCtts(samples);
        if (ctts is not null) children.Add(ctts);
        children.Add(BuildStsc());
        children.Add(BuildStsz(samples));
        children.Add(BuildStco(samples.Count));

        output = MakeBox(0x7374626C /*stbl*/, Concat(children));
        return true;
    }

    /// <summary>Offset of the (single) stco entry array inside a rebuilt moov box.</summary>
    private static int FindStcoPayloadIndex(byte[] moovBytes)
    {
        if (moovBytes.Length < 8) return -1;

        foreach (var trak in Children(moovBytes, 8, moovBytes.Length - 8))
        {
            if (Type(trak, moovBytes) != 0x7472616B) continue;
            var mdia = FindChild(moovBytes, trak.Offset + 8, trak.Size - 8, 0x6D646961);
            if (mdia is null) continue;
            var minf = FindChild(moovBytes, mdia.Value.Offset + 8, mdia.Value.Size - 8, 0x6D696E66);
            if (minf is null) continue;
            var stbl = FindChild(moovBytes, minf.Value.Offset + 8, minf.Value.Size - 8, 0x7374626C);
            if (stbl is null) continue;
            var stco = FindChild(moovBytes, stbl.Value.Offset + 8, stbl.Value.Size - 8, 0x7374636F);
            if (stco is null) continue;

            // header(8) + version/flags(4) + entry_count(4)
            return stco.Value.Offset + 16;
        }

        return -1;
    }

    private static void WriteBox(Stream stream, uint type, byte[] payload)
    {
        Span<byte> header = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(header[..4], (uint)(8 + payload.Length));
        BinaryPrimitives.WriteUInt32BigEndian(header[4..], type);
        stream.Write(header);
        stream.Write(payload);
    }
}
