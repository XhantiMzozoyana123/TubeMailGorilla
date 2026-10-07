using System.Diagnostics;
using System.Text;
using SQLite;
using TubeMailGorilla.Maui.Unlocked.Services;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;

namespace SnapDiag;

/// <summary>
/// Stage-by-stage diagnostic of the video snapshot pipeline, replicating the
/// exact code in VideoSnapshotService / MediaFoundationFrameExtractor.
/// Exit code 0 = all stages pass, 1 = a stage failed.
/// </summary>
public static class Program
{
    // Same values as VideoSnapshotService.
    const double SnapshotIntervalSeconds = 10;
    const int MaxSnapshotsPerVideo = 30;
    const int FrameWidth = 320;
    const int FrameHeight = 180;
    static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(4);

    public static async Task<int> Main(string[] args)
    {
        var log = new StringBuilder();
        void Say(string s)
        {
            Console.WriteLine(s);
            log.AppendLine(s);
        }

        var failed = false;
        var dbPath = args.Length > 0
            ? args[0]
            : @"C:\Users\Shadow\AppData\Local\User Name\com.tubemailgorilla.maui.unlocked\Data\TubeMailGorillaUnlocked\tubemailgorilla.db3";

        // ---------------------------------------------------------- stage 1
        Say($"=== stage 1: read DB {dbPath}");
        string? videoUrl = null;
        try
        {
            using var db = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadOnly);
            var total = db.ExecuteScalar<int>("select count(*) from EmailContact");
            var withSnaps = db.ExecuteScalar<int>(
                "select count(*) from EmailContact where VideoSnapshotJson is not null and VideoSnapshotJson != ''");
            Say($"contacts={total} withSnapshots={withSnaps}");
            videoUrl = db.ExecuteScalar<string?>(
                "select VideoUrl from EmailContact where VideoUrl is not null and VideoUrl != '' order by Id desc limit 1");
            Say($"sample videoUrl={videoUrl}");
        }
        catch (Exception ex)
        {
            Say($"DB FAILED: {ex.Message}");
            failed = true;
        }

        if (videoUrl is null)
        {
            Say("no video url to test with - stopping");
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "snapdiag.txt"), log.ToString());
            return 1;
        }

        // ---------------------------------------------------------- stage 2
        Say("=== stage 2: YoutubeExplode manifest (DownloadVideoAsync)");
        IStreamInfo? stream = null;
        try
        {
            using var timeout = new CancellationTokenSource(DownloadTimeout);
            var youtube = new YoutubeClient();
            var videoId = VideoId.TryParse(videoUrl);
            Say($"VideoId.TryParse -> {videoId}");
            if (videoId is null) throw new InvalidOperationException("VideoId.TryParse returned null");

            var sw = Stopwatch.StartNew();
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Value, timeout.Token);
            sw.Stop();
            Say($"manifest ok in {sw.ElapsedMilliseconds} ms: muxed={manifest.GetMuxedStreams().Count()} " +
                $"videoOnly={manifest.GetVideoOnlyStreams().Count()} audioOnly={manifest.GetAudioOnlyStreams().Count()}");

            stream = (IStreamInfo?)manifest.GetMuxedStreams()
                        .OrderBy(s => s.VideoResolution.Height).ThenBy(s => s.Bitrate).FirstOrDefault()
                    ?? manifest.GetVideoOnlyStreams()
                        .OrderBy(s => s.VideoResolution.Height).ThenBy(s => s.Bitrate).FirstOrDefault();
            Say(stream is null
                ? "NO stream found"
                : $"stream chosen: {stream.GetType().Name} {stream}");
        }
        catch (Exception ex)
        {
            Say($"MANIFEST FAILED: {ex.GetType().Name}: {ex.Message}");
            failed = true;
        }

        // ---------------------------------------------------------- stage 3
        Say("=== stage 3: download stream");
        string? videoPath = null;
        if (stream is not null)
        {
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "snapdiag_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                videoPath = Path.Combine(tempDir, "video.mp4");

                using var timeout = new CancellationTokenSource(DownloadTimeout);
                var sw = Stopwatch.StartNew();
                await new YoutubeClient().Videos.Streams.DownloadAsync(stream, videoPath, null, timeout.Token);
                sw.Stop();

                Say($"download ok in {sw.ElapsedMilliseconds} ms: {new FileInfo(videoPath).Length} bytes");
            }
            catch (Exception ex)
            {
                Say($"DOWNLOAD FAILED: {ex.GetType().Name}: {ex.Message}");
                failed = true;
            }
        }
        else
        {
            Say("skipped (no stream)");
        }

        // ---------------------------------------------------------- stage 4
        Say("=== stage 4: MediaFoundation frame extraction (variant matrix)");
        if (videoPath is not null)
        {
            try
            {
                var ok = await ExtractFramesLikeApp(videoPath, Say);
                if (!ok) failed = true;
            }
            catch (Exception ex)
            {
                Say($"EXTRACT FAILED: {ex.GetType().Name}: {ex.Message}");
                failed = true;
            }
        }
        else
        {
            Say("skipped (no video file)");
        }

        // ---------------------------------------------------------- stage 5
        var localMp4 = @"c:\Users\Shadow\Documents\TubeMailGorilla\TubeMailGorilla.Web\src\videos\tubemailgorilla-app-demo-1.mp4";
        Say("=== stage 5: same probe on a known-good local mp4");
        if (File.Exists(localMp4))
        {
            try
            {
                var ok5 = await ExtractFramesLikeApp(localMp4, Say);
                if (!ok5) failed = true;
            }
            catch (Exception ex)
            {
                Say($"LOCAL EXTRACT FAILED: {ex.GetType().Name}: {ex.Message}");
                failed = true;
            }
        }
        else
        {
            Say("local mp4 missing - skipped");
        }

        // ---------------------------------------------------------- stage 6
        // Fix candidate: transcode the fragmented download to a plain MP4 with
        // the OS MediaTranscoder, then retry the same thumbnail probes.
        Say("=== stage 6: MediaTranscoder fMP4 -> mp4, then probe");
        if (videoPath is not null)
        {
            try
            {
                var src = await StorageFile.GetFileFromPathAsync(videoPath);
                var dstPath = Path.Combine(Path.GetDirectoryName(videoPath)!, "transcoded.mp4");
                var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(videoPath)!);
                var dst = await folder.CreateFileAsync("transcoded.mp4", CreationCollisionOption.ReplaceExisting);

                var transcoder = new Windows.Media.Transcoding.MediaTranscoder();
                var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
                // The download is video-only; a profile that demands an audio
                // track makes the audio sink fail with "no samples".
                profile.Audio = null;
                var prep = await transcoder.PrepareFileTranscodeAsync(src, dst, profile);
                Say($"  CanTranscode={prep.CanTranscode}");
                if (prep.CanTranscode)
                {
                    var swT = Stopwatch.StartNew();
                    await prep.TranscodeAsync();
                    Say($"  transcoded in {swT.ElapsedMilliseconds} ms -> {new FileInfo(dstPath).Length} bytes");

                    var ok6 = await ExtractFramesLikeApp(dstPath, Say);
                    if (!ok6) failed = true;
                }
                else
                {
                    Say("  Cannot transcode - fix candidate fails");
                    failed = true;
                }
            }
            catch (Exception ex)
            {
                Say($"TRANSCODE FAILED: {ex.GetType().Name}: {ex.Message}");
                failed = true;
            }
        }
        else
        {
            Say("skipped (no video file)");
        }

        // ---------------------------------------------------------- stage 7
        // THE FIX: fold the fragments into a progressive MP4, then probe.
        Say("=== stage 7: managed fMP4 remux -> mp4, then probe");
        if (videoPath is not null)
        {
            try
            {
                var fragmented = Mp4FragmentRemuxer.LooksFragmented(videoPath);
                Say($"  LooksFragmented={fragmented}");
                var flatPath = Path.Combine(Path.GetDirectoryName(videoPath)!, "video_flat.mp4");
                var okRemux = await Task.Run(() => Mp4FragmentRemuxer.TryRemux(videoPath, flatPath));
                Say($"  TryRemux={okRemux} outSize={(okRemux ? new FileInfo(flatPath).Length.ToString() : "n/a")}");
                if (okRemux)
                {
                    var ok7 = await ExtractFramesLikeApp(flatPath, Say);
                    if (!ok7) failed = true;
                }
                else
                {
                    failed = true;
                }
            }
            catch (Exception ex)
            {
                Say($"REMUX FAILED: {ex.GetType().Name}: {ex.Message}");
                failed = true;
            }
        }
        else
        {
            Say("skipped (no video file)");
        }

        Say(failed ? "RESULT: FAIL" : "RESULT: PASS");
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "snapdiag.txt"), log.ToString());
        return failed ? 1 : 0;
    }

    /// <summary>Variant matrix around MediaFoundationFrameExtractor.ExtractFrameAsync.</summary>
    static async Task<bool> ExtractFramesLikeApp(string videoPath, Action<string> say)
    {
        var file = await StorageFile.GetFileFromPathAsync(videoPath);
        var clip = await MediaClip.CreateFromFileAsync(file);
        var composition = new MediaComposition();
        composition.Clips.Add(clip);
        var duration = clip.OriginalDuration;
        say($"open ok: clip.OriginalDuration={duration}");
        say($"  composition.Duration={composition.Duration} composition.Clips.Count={composition.Clips.Count}");

        if (duration <= TimeSpan.Zero)
        {
            say("duration <= 0");
            return false;
        }

        var anyOk = false;

        // The app's exact call first.
        anyOk |= await Probe(composition, 1.0, FrameWidth, FrameHeight, VideoFramePrecision.NearestFrame, say);

        // Variants to isolate the cause.
        anyOk |= await Probe(composition, 1.0, 256, 144, VideoFramePrecision.NearestFrame, say);
        anyOk |= await Probe(composition, 1.0, 128, 72, VideoFramePrecision.NearestFrame, say);
        anyOk |= await Probe(composition, 0.0, FrameWidth, FrameHeight, VideoFramePrecision.NearestFrame, say);
        anyOk |= await Probe(composition, 10.0, FrameWidth, FrameHeight, VideoFramePrecision.NearestFrame, say);
        anyOk |= await Probe(composition, 1.0, -1, -1, VideoFramePrecision.NearestFrame, say);

        // Fresh composition in case a failed call poisons the first one.
        var composition2 = new MediaComposition();
        composition2.Clips.Add(await MediaClip.CreateFromFileAsync(file));
        anyOk |= await Probe(composition2, 1.0, FrameWidth, FrameHeight, VideoFramePrecision.NearestFrame, say);

        // Full end-to-end sweep exactly as the app does it (BuildTimestamps +
        // per-frame clamp + timeout + JPEG read).
        try
        {
            var timestamps = BuildTimestamps(duration);
            var captured = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            foreach (var seconds in timestamps)
            {
                var target = Math.Clamp(seconds, 0.1, Math.Max(0.1, duration.TotalSeconds - 0.1));
                try
                {
                    using var frameTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                    var thumb = await composition2.GetThumbnailAsync(
                        TimeSpan.FromSeconds(target), FrameWidth, FrameHeight, VideoFramePrecision.NearestFrame);
                    var size = (int)thumb.Size;
                    var reader = new DataReader(thumb.GetInputStreamAt(0));
                    await reader.LoadAsync((uint)size);
                    var bytes = new byte[size];
                    reader.ReadBytes(bytes);
                    reader.Dispose();
                    thumb.Dispose();
                    if (bytes.Length > 0) captured++;
                }
                catch { }
            }
            sw.Stop();
            say($"  full-loop sweep: captured {captured}/{timestamps.Count} in {sw.ElapsedMilliseconds} ms");
            anyOk |= captured > 0;
        }
        catch (Exception ex)
        {
            say($"  sweep setup FAILED: {ex.Message}");
        }

        say(anyOk ? "at least one variant OK" : "ALL variants failed");
        return anyOk;
    }

    /// <summary>Exact copy of VideoSnapshotService.BuildTimestamps.</summary>
    internal static IReadOnlyList<double> BuildTimestamps(TimeSpan duration)
    {
        var total = duration.TotalSeconds;
        if (total <= 0) return Array.Empty<double>();

        var first = Math.Min(1d, total / 2d);
        var last = Math.Max(first, total - 0.5);

        var wanted = (int)Math.Floor((last - first) / SnapshotIntervalSeconds) + 1;
        if (wanted <= 0) return new[] { first };

        var step = wanted <= MaxSnapshotsPerVideo
            ? SnapshotIntervalSeconds
            : (last - first) / (MaxSnapshotsPerVideo - 1);

        var result = new List<double>(Math.Min(wanted, MaxSnapshotsPerVideo));
        for (var i = 0; i < wanted && result.Count < MaxSnapshotsPerVideo; i++)
        {
            var seconds = first + i * step;
            if (seconds > last) break;
            result.Add(Math.Round(seconds, 2));
        }

        return result;
    }

    static async Task<bool> Probe(
        MediaComposition composition, double seconds, int width, int height,
        VideoFramePrecision precision, Action<string> say)
    {
        try
        {
            using var frameTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var thumb2 = await composition.GetThumbnailAsync(
                TimeSpan.FromSeconds(seconds), width, height, precision);
            try
            {
                var size2 = (int)thumb2.Size;
                say($"  t={seconds} {width}x{height} {precision}: OK size={size2} type={thumb2.ContentType}");
                return size2 > 0;
            }
            finally { thumb2.Dispose(); }
        }
        catch (Exception ex)
        {
            say($"  t={seconds} {width}x{height} {precision}: FAIL {ex.GetType().Name} hr=0x{ex.HResult:X8}: {ex.Message}");
            if (ex.StackTrace is not null)
                say("    " + ex.StackTrace.Replace("\r", "").Replace("\n", " | "));
            return false;
        }
    }
}
