using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.ClosedCaptions;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// YouTube data access built on the <see cref="YoutubeClient"/> library
/// (YoutubeExplode) — no bundled yt-dlp binary and no HTML scraping.
///
/// Replaces the previous process-based pipeline:
///   - yt-dlp flat-playlist search  -> <see cref="SearchAsync"/>
///   - yt-dlp --get-description     -> <see cref="GetDescriptionAsync"/>
///   - yt-dlp --write-auto-sub      -> <see cref="GetTranscriptAsync"/>
/// </summary>
public class YouTubeExplodeService
{
    private readonly YoutubeClient _youtube = new();

    /// <summary>
    /// Resolves up to <paramref name="maxResults"/> YouTube videos for a query
    /// using YoutubeExplode's search. Returns whatever could be resolved; an
    /// empty list means "no results" and callers treat it as such.
    /// </summary>
    public async Task<List<YouTubeVideo>> SearchAsync(string query, int maxResults = 10)
    {
        try
        {
            var results = await _youtube.Search.GetVideosAsync(query).CollectAsync(maxResults);

            return results
                .Select(r => new YouTubeVideo
                {
                    VideoId = r.Id.Value,
                    Url = $"https://www.youtube.com/watch?v={r.Id.Value}",
                    Title = r.Title,
                    Author = string.IsNullOrWhiteSpace(r.Author?.ChannelTitle) ? null : r.Author.ChannelTitle,
                })
                .ToList();
        }
        catch
        {
            // Search failed entirely; caller treats an empty list as "no results".
            return new List<YouTubeVideo>();
        }
    }

    /// <summary>Fetches the watch-page description for a video (empty on failure).</summary>
    public async Task<string> GetDescriptionAsync(string videoUrl)
    {
        try
        {
            var video = await _youtube.Videos.GetAsync(videoUrl);
            return video.Description ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Fetches auto-generated or uploader-provided closed captions for a video
    /// and returns them as plain text (empty if none are available).
    /// </summary>
    public async Task<string> GetTranscriptAsync(string videoUrl, string lang = "en")
    {
        try
        {
            var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);

            var track = manifest.TryGetByLanguage(lang)
                        ?? manifest.Tracks.FirstOrDefault()
                        ?? manifest.Tracks.FirstOrDefault(t =>
                            t.Language.Code.StartsWith(lang, StringComparison.OrdinalIgnoreCase));

            if (track is null)
                return string.Empty;

            var trackContent = await _youtube.Videos.ClosedCaptions.GetAsync(track);
            return string.Join(" ", trackContent.Captions.Select(c => c.Text));
        }
        catch
        {
            return string.Empty;
        }
    }
}

/// <summary>
/// A YouTube video resolved from search results (previously defined in the
/// yt-dlp based YouTubeSearchService, now produced by YouTubeExplodeService).
/// </summary>
public class YouTubeVideo
{
    public string VideoId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public long? ViewCount { get; set; }
}