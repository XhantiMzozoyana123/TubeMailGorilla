using System.Text.RegularExpressions;
using TubeMailGorillaApplication.Interfaces;

namespace TubeMailGorillaInfrastructure.Services
{
    public class CaptionService : ICaptionService
    {
        private static readonly HttpClient _http = new();

        public async Task<string> ExtractCaptionsAsync(string videoUrl)
        {
            try
            {
                var videoId = ExtractVideoId(videoUrl);

                // Fetch the YouTube page to find caption track URLs
                var pageContent = await _http.GetStringAsync($"https://www.youtube.com/watch?v={videoId}");

                // Extract caption track URLs from the page source (timedtext URLs)
                var matches = Regex.Matches(pageContent, @"""baseUrl"":\s*""(https://www\.youtube\.com/api/timedtext[^""]+)""");

                if (!matches.Any())
                    return string.Empty;

                // Prefer English captions
                var captionUrl = matches
                    .Select(m => m.Groups[1].Value.Replace("\\u0026", "&"))
                    .FirstOrDefault(u => u.Contains("lang=en"))
                    ?? matches.First().Groups[1].Value.Replace("\\u0026", "&");

                // Download the caption content
                var captionContent = await _http.GetStringAsync(captionUrl);

                // Parse XML format and extract plain text
                return ExtractPlainTextFromCaptions(captionContent);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ExtractVideoId(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var host = uri.Host.ToLowerInvariant();
                var path = uri.AbsolutePath;

                if (host.Contains("youtube.com") && path.Contains("/watch"))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var v = query["v"];
                    if (!string.IsNullOrEmpty(v)) return v;
                }

                if (host == "youtu.be" && !string.IsNullOrEmpty(path.TrimStart('/')))
                    return path.TrimStart('/');

                if (path.Contains("/embed/"))
                    return path.Split("/embed/").Last().Split('?').First();

                if (path.Contains("/shorts/"))
                    return path.Split("/shorts/").Last().Split('?').First();
            }

            if (url.Length == 11 && url.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
                return url;

            throw new ArgumentException($"Could not extract video ID from URL: {url}");
        }

        private static string ExtractPlainTextFromCaptions(string captionContent)
        {
            // Handle XML format from YouTube timedtext API
            var textMatches = Regex.Matches(captionContent, @"<text[^>]*>([^<]+)</text>");
            if (textMatches.Any())
            {
                return string.Join(" ",
                    textMatches.Select(m => System.Net.WebUtility.HtmlDecode(m.Groups[1].Value)));
            }

            // Handle VTT/SRT format fallback
            var lines = captionContent.Split('\n');
            var textLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                if (trimmed.StartsWith("WEBVTT")) continue;
                if (trimmed.StartsWith("Kind:") || trimmed.StartsWith("Language:")) continue;
                if (trimmed.Contains("-->")) continue;
                if (int.TryParse(trimmed, out _)) continue;

                var clean = Regex.Replace(trimmed, "<[^>]+>", "");
                if (!string.IsNullOrWhiteSpace(clean))
                    textLines.Add(clean);
            }

            return string.Join(" ", textLines.Distinct());
        }
    }
}