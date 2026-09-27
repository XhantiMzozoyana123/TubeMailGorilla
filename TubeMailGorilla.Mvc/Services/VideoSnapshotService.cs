using System.Diagnostics;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace TubeMailGorilla.Mvc.Services;

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
/// Frame decoding is done by <c>ffmpeg</c> (see <see cref="Ffmpeg"/>). The
/// desktop edition uses the platform's own decoder instead - Windows Media
/// Foundation or macOS AVFoundation - because those do not exist on a Linux VPS.
/// The cadence, frame size, cap and the resulting <see cref="VideoSnapshot"/>
/// shape are identical across both, so the lead data is interchangeable.
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

    /// <summary>
    /// Seconds ffmpeg is told to seek before it starts sampling - frame 0 is
    /// almost always black, so the timeline starts just after it.
    /// </summary>
    private const double StartOffsetSeconds = 1;

    /// <summary>Never spend longer than this downloading one video.</summary>
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(4);

    /// <summary>Whole-capture budget for the ffmpeg pass.</summary>
    private static readonly TimeSpan ExtractTimeout = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Serialises capture. Extraction already runs one video at a time, and the
    /// ffmpeg process is heavy enough that a second concurrent pass would just
    /// make both slower.
    /// </summary>
    private static readonly SemaphoreSlim CaptureLock = new(1, 1);

    /// <summary>
    /// Takes snapshots every 10 seconds of the video, in order.
    /// Never throws: a video that cannot be downloaded or decoded simply yields
    /// fewer (or zero) images and extraction carries on.
    /// </summary>
    public async Task<List<VideoSnapshot>> CaptureAsync(string videoUrl, CancellationToken cancellationToken = default)
    {
        // Without a decoder there is nothing to try, and spawning a process per
        // video that is guaranteed to fail is pure cost.
        if (!Ffmpeg.IsAvailable)
            return new List<VideoSnapshot>();

        var tempDir = Path.Combine(Path.GetTempPath(), "tmg_snap_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            await CaptureLock.WaitAsync(cancellationToken);
            try
            {
                var videoPath = await DownloadVideoAsync(videoUrl, tempDir, cancellationToken);
                if (videoPath == null) return new List<VideoSnapshot>();

                return await ExtractFramesAsync(videoPath, tempDir, cancellationToken);
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

    /// <summary>
    /// Runs a single ffmpeg pass that samples one frame every
    /// <see cref="SnapshotIntervalSeconds"/> seconds and writes them as numbered
    /// JPEGs.
    ///
    /// One pass with an <c>fps</c> filter is used rather than one
    /// seek-and-grab process per timestamp: seeking repeatedly re-opens and
    /// re-decodes the file from the start each time, which on a 10 minute video
    /// costs far more than decoding it once straight through.
    /// </summary>
    private static async Task<List<VideoSnapshot>> ExtractFramesAsync(string videoPath, string tempDir, CancellationToken cancellationToken)
    {
        var snapshots = new List<VideoSnapshot>();

        // A pattern, not a file: %04d is ffmpeg's own sequence numbering. The
        // fps filter decides which frames are emitted, so the sequence number is
        // NOT the timestamp - the times are rebuilt from the index below.
        var outputPattern = Path.Combine(tempDir, "frame_%04d.jpg");
        var fps = 1d / SnapshotIntervalSeconds;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = Ffmpeg.ResolvedPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // -ss before -i seeks by keyframe (fast); the fps filter then walks
            // forward from there at the fixed cadence.
            psi.ArgumentList.Add("-hide_banner");
            psi.ArgumentList.Add("-loglevel");
            psi.ArgumentList.Add("error");
            psi.ArgumentList.Add("-ss");
            psi.ArgumentList.Add(StartOffsetSeconds.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
            psi.ArgumentList.Add("-i");
            psi.ArgumentList.Add(videoPath);
            psi.ArgumentList.Add("-vf");
            psi.ArgumentList.Add($"fps={fps.ToString("0.########", System.Globalization.CultureInfo.InvariantCulture)},scale={FrameWidth}:{FrameHeight}");
            psi.ArgumentList.Add("-frames:v");
            psi.ArgumentList.Add(MaxSnapshotsPerVideo.ToString());
            psi.ArgumentList.Add("-q:v");
            psi.ArgumentList.Add("5");
            psi.ArgumentList.Add("-y");
            psi.ArgumentList.Add(outputPattern);

            using var process = Process.Start(psi);
            if (process is null) return snapshots;

            // Drain both pipes concurrently: ffmpeg blocks writing stderr, and
            // leaving it unread can deadlock the child once its buffer fills.
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(ExtractTimeout);

            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                // A video that will not finish in the budget is abandoned, but
                // the frames ffmpeg already wrote are still valid and are read
                // below, so a partial timeline beats none at all.
                TryKill(process);
            }

            await Task.WhenAll(stdoutTask, stderrTask);
        }
        catch
        {
            // No decoder, or the binary could not be started. Fall through: any
            // frames already on disk are still picked up.
        }

        var files = Directory.GetFiles(tempDir, "frame_*.jpg")
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToList();

        for (var i = 0; i < files.Count; i++)
        {
            try
            {
                var jpeg = await File.ReadAllBytesAsync(files[i], cancellationToken);
                if (jpeg.Length == 0) continue;

                // Frame 0 was taken at the seek offset, so the timestamps carry
                // the same offset rather than starting at zero.
                var seconds = StartOffsetSeconds + (i * SnapshotIntervalSeconds);
                snapshots.Add(new VideoSnapshot(Convert.ToBase64String(jpeg), Math.Round(seconds, 2)));
            }
            catch
            {
                // One unreadable frame must not abandon the rest of the video.
            }
        }

        return snapshots;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Already gone.
        }
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