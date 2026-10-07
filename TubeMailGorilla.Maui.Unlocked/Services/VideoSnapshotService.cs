using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Downloads each lead's video with YouTubeExplode and captures a still frame
/// every <see cref="SnapshotIntervalSeconds"/> seconds, so a lead carries a
/// visual timeline of the video its contact info came from.
///
/// Why a fixed interval rather than the transcript timestamps: caption timings
/// depend on auto-captions being present and well-formed, and a video with no
/// captions produced no snapshots at all. A plain fixed cadence works for every
/// video.
///
/// Frame decoding uses the platform's own decoder (see
/// <see cref="IVideoFrameExtractor"/>) - Windows Media Foundation or macOS
/// AVFoundation - so this needs no ffmpeg install and no bundled binary.
/// </summary>
public class VideoSnapshotService
{
    /// <summary>Seconds between snapshots.</summary>
    public const double SnapshotIntervalSeconds = 10;

    /// <summary>Frame width in pixels (height follows the 16:9 aspect ratio).</summary>
    private const int FrameWidth = 320;

    private const int FrameHeight = 180;

    /// <summary>
    /// Hard cap on snapshots per lead. An hour-long video at a 10s interval
    /// would be 360 frames, far too much to keep per contact; the cap keeps the
    /// database and the editor carousel usable.
    /// </summary>
    public const int MaxSnapshotsPerVideo = 30;

    /// <summary>Never spend longer than this downloading one video.</summary>
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(4);

    /// <summary>Per-frame timeout, so one bad seek cannot stall a run.</summary>
    private static readonly TimeSpan FrameTimeout = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Serialises capture. The platform decoders are single-instance and heavy,
    /// and extraction already runs one video at a time.
    /// </summary>
    private static readonly SemaphoreSlim CaptureLock = new(1, 1);

    /// <summary>
    /// Takes snapshots every 10 seconds of the video, in order.
    /// Never throws: a video that cannot be downloaded or decoded simply yields
    /// fewer (or zero) images and extraction carries on.
    /// </summary>
    public async Task<List<VideoSnapshot>> CaptureAsync(string videoUrl, CancellationToken cancellationToken = default)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "tmg_snap_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            await CaptureLock.WaitAsync(cancellationToken);
            try
            {
                var videoPath = await DownloadVideoAsync(videoUrl, tempDir, cancellationToken);
                if (videoPath == null) return new List<VideoSnapshot>();

                // YouTube serves its DASH streams as fragmented MP4, and
                // Windows Media Foundation cannot serve samples from those -
                // every thumbnail seek fails and the lead ends up with no
                // frames at all. Fold the fragments into a plain progressive
                // MP4 first (managed, no ffmpeg - see Mp4FragmentRemuxer);
                // progressive downloads skip this and decode directly.
                var workPath = videoPath;
                if (Mp4FragmentRemuxer.LooksFragmented(videoPath))
                {
                    var flatPath = Path.Combine(tempDir, "video_flat.mp4");
                    var remuxed = await Task.Run(
                        () => Mp4FragmentRemuxer.TryRemux(videoPath, flatPath),
                        cancellationToken);

                    // A failed remux keeps the original: worse case is the old
                    // behaviour, never a lost video.
                    if (remuxed) workPath = flatPath;
                }

                return await ExtractFramesAsync(workPath, cancellationToken);
            }
            finally
            {
                CaptureLock.Release();
            }
        }
        catch
        {
            return new List<VideoSnapshot>();
        }
        finally
        {
            TryDelete(tempDir);
        }
    }

    // ------------------------------------------------------------------
    //  Download
    // ------------------------------------------------------------------

    /// <summary>
    /// Fetches a low-resolution copy of the video into the temp folder. Small is
    /// deliberate: 320px snapshots need no more, and it keeps a multi-video
    /// extraction to a sensible amount of traffic.
    /// </summary>
    private static async Task<string?> DownloadVideoAsync(string videoUrl, string tempDir, CancellationToken cancellationToken)
    {
        try
        {
            // VideoId.TryParse returns null for anything that is not a video id
            // or a watch/short URL, which is how a bad row is filtered out.
            var videoId = VideoId.TryParse(videoUrl);
            if (videoId is null) return null;

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(DownloadTimeout);

            var youtube = new YoutubeClient();
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Value, timeout.Token);

            // Muxed streams carry audio + video in one file, so there is no
            // separate audio file to stitch in. Prefer the smallest one: 320px
            // snapshots need no more, and it keeps the download quick.
            // Each side is cast because ?? binds before the target type is
            // applied, and the two stream kinds are different concrete types.
            IStreamInfo? stream = (IStreamInfo?)manifest.GetMuxedStreams()
                                          .OrderBy(s => s.VideoResolution.Height)
                                          .ThenBy(s => s.Bitrate)
                                          .FirstOrDefault()
                                      ?? manifest.GetVideoOnlyStreams()
                                          .OrderBy(s => s.VideoResolution.Height)
                                          .ThenBy(s => s.Bitrate)
                                          .FirstOrDefault();

            if (stream is null) return null;

            var path = Path.Combine(tempDir, "video.mp4");
            await youtube.Videos.Streams.DownloadAsync(stream, path, null, timeout.Token);

            return File.Exists(path) ? path : null;
        }
        catch
        {
            return null;
        }
    }

    // ------------------------------------------------------------------
    //  Frame extraction
    // ------------------------------------------------------------------

    private static async Task<List<VideoSnapshot>> ExtractFramesAsync(string videoPath, CancellationToken cancellationToken)
    {
        var snapshots = new List<VideoSnapshot>();

        using var extractor = VideoFrameExtractorFactory.Create();

        if (!await extractor.OpenAsync(videoPath, cancellationToken))
            return snapshots;

        if (extractor.Duration <= TimeSpan.Zero) return snapshots;

        foreach (var seconds in BuildTimestamps(extractor.Duration))
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[]? jpeg = null;
            try
            {
                using var frameTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                frameTimeout.CancelAfter(FrameTimeout);

                jpeg = await extractor.ExtractFrameAsync(seconds, FrameWidth, FrameHeight, frameTimeout.Token);
            }
            catch
            {
                // One undecodable position must not abort the rest of the video.
            }

            if (jpeg is null || jpeg.Length == 0) continue;

            snapshots.Add(new VideoSnapshot(Convert.ToBase64String(jpeg), seconds));
        }

        return snapshots;
    }

    /// <summary>
    /// The seek positions to capture: every 10 seconds, starting a little way in
    /// (frame 0 is nearly always black) and stopping just short of the end.
    ///
    /// If the video is long enough to exceed <see cref="MaxSnapshotsPerVideo"/>,
    /// the frames are thinned out evenly instead, so the set still spans the whole
    /// video rather than bunching at the start.
    /// </summary>
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

            // Guard against float drift pushing a seek past the end.
            if (seconds > last) break;

            result.Add(Math.Round(seconds, 2));
        }

        return result;
    }

    private static void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
        catch
        {
            // A locked temp file must never fail an extraction.
        }
    }
}

/// <summary>
/// A single captured frame plus the video position it came from, so the lead
/// editor can label the image with the moment it was taken.
/// </summary>
public class VideoSnapshot
{
    public VideoSnapshot(string base64Jpeg, double seconds)
    {
        Base64Image = base64Jpeg;
        Seconds = seconds;
    }

    /// <summary>Base64-encoded JPEG data (no data-URI prefix).</summary>
    public string Base64Image { get; }

    /// <summary>Position in the video this frame was taken at.</summary>
    public double Seconds { get; }

    public string TimestampLabel => TranscriptCue.FormatTimestamp(Seconds);
}
