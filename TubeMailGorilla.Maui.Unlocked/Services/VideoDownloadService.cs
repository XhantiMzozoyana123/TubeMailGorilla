using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Downloads a lead's full-length source video as a single MP4 file the user
/// can keep. Uses the same <see cref="YoutubeClient"/> pipeline as
/// <see cref="VideoSnapshotService"/>, but picks the highest-resolution
/// <em>muxed</em> stream (audio + video in one file) so the result plays
/// everywhere with no merging step.
/// </summary>
public static class VideoDownloadService
{
    /// <summary>Full videos are far bigger than snapshot clips - allow longer.</summary>
    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Fetches the video into a temp file. Returns the temp path plus a
    /// suggested file name, or null when the video cannot be downloaded.
    /// The caller owns cleanup of the temp folder afterwards.
    /// </summary>
    public static async Task<(string TempPath, string SuggestedFileName)?> DownloadAsync(
        string videoUrl,
        string? titleHint,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var videoId = VideoId.TryParse(videoUrl);
        if (videoId is null) return null;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(DownloadTimeout);
        var token = timeout.Token;

        try
        {
            var youtube = new YoutubeClient();
            var manifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Value, token);

            // Muxed streams carry audio + video in one MP4, so there is
            // nothing to stitch. Take the sharpest one available.
            var stream = manifest.GetMuxedStreams()
                .OrderByDescending(s => s.VideoResolution.Height)
                .ThenByDescending(s => s.Bitrate)
                .FirstOrDefault();

            if (stream is null) return null;

            // Prefer the real video title for the file name; fall back to
            // whatever the caller knew (the lead's stored title).
            var title = titleHint ?? string.Empty;
            try
            {
                var video = await youtube.Videos.GetAsync(videoId.Value, token);
                if (!string.IsNullOrWhiteSpace(video.Title))
                    title = video.Title;
            }
            catch
            {
                // Title lookup is cosmetic - the stored hint still works.
            }

            var suggested = BuildFileName(title);
            var tempDir = Path.Combine(Path.GetTempPath(), "tmg_vid_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var tempPath = Path.Combine(tempDir, suggested);

            await youtube.Videos.Streams.DownloadAsync(stream, tempPath, progress, token);

            return File.Exists(tempPath) ? (tempPath, suggested) : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// A safe, readable file name. Video titles come from scraped YouTube
    /// metadata, so anything illegal in a filename has to go.
    /// </summary>
    private static string BuildFileName(string title)
    {
        var source = string.IsNullOrWhiteSpace(title) ? "video" : title;

        var safe = new string(source
            .Select(ch => char.IsLetterOrDigit(ch) || ch is ' ' or '-' or '_' ? ch : '-')
            .ToArray())
            .Trim('-', ' ');

        if (safe.Length == 0) safe = "video";
        if (safe.Length > 60) safe = safe[..60].Trim('-', ' ');

        return $"{safe}.mp4";
    }
}
