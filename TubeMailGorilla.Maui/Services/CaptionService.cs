using System.Text.RegularExpressions;

namespace TubeMailGorilla.Maui.Services;

public class CaptionService
{
    private static readonly HttpClient httpClient = new();
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    public async Task<string> ExtractCaptionsAsync(string videoUrl)
    {
        try
        {
            var videoId = ExtractVideoId(videoUrl);
            var watchUrl = $"https://www.youtube.com/watch?v={videoId}";
            
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            var response = await httpClient.GetStringAsync(watchUrl);
            
            // Look for caption tracks in page JSON
            var baseUrlPattern = @"""baseUrl"":\s*""(https:\\/\\/www\\.youtube\\.com\\/api\\/timedtext[^""]+)""";
            var matches = Regex.Matches(response, baseUrlPattern);
            
            if (matches.Count == 0)
                return string.Empty;
            
            string? captionUrl = null;
            foreach (Match match in matches)
            {
                var url = match.Groups[1].Value.Replace("\\u0026", "&");
                if (url.Contains("lang=en"))
                {
                    captionUrl = url;
                    break;
                }
            }
            
            captionUrl ??= matches[0].Groups[1].Value.Replace("\\u0026", "&");
            
            var captionResponse = await httpClient.GetStringAsync(captionUrl);
            return ExtractPlainTextFromCaptions(captionResponse);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ExtractVideoId(string url)
    {
        try
        {
            var uri = new Uri(url);
            if (uri.Host.Contains("youtube.com") && uri.LocalPath.Contains("/watch"))
            {
                var v = uri.Query.Split('&').FirstOrDefault(q => q.StartsWith("v="))?.Substring(2);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            if (uri.Host == "youtu.be" && uri.LocalPath.Length > 1)
                return uri.LocalPath.Substring(1);
            if (uri.LocalPath.Contains("/embed/"))
                return uri.LocalPath.Split("/embed/")[1].Split('?')[0];
            if (uri.LocalPath.Contains("/shorts/"))
                return uri.LocalPath.Split("/shorts/")[1].Split('?')[0];
        }
        catch { }
        
        if (url.Length == 11 && Regex.IsMatch(url, @"^[a-zA-Z0-9_-]+$"))
            return url;
            
        throw new Exception($"Could not extract video ID from URL: {url}");
    }

    private static string ExtractPlainTextFromCaptions(string captionContent)
    {
        var textMatches = Regex.Matches(captionContent, @"<text[^>]*>([^<]+)</text>");
        var texts = new List<string>();
        foreach (Match match in textMatches)
        {
            var text = match.Groups[1].Value
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&#39;", "'");
            texts.Add(text);
        }
        
        if (texts.Count > 0)
            return string.Join(" ", texts);
        
        var lines = captionContent.Split('\n');
        var textLines = new List<string>();
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            if (trimmed.StartsWith("WEBVTT")) continue;
            if (trimmed.StartsWith("Kind:") || trimmed.StartsWith("Language:")) continue;
            if (trimmed.Contains("-->")) continue;
            if (Regex.IsMatch(trimmed, @"^\d+$")) continue;
            
            var clean = Regex.Replace(trimmed, "<[^>]+>", "");
            if (!string.IsNullOrEmpty(clean))
                textLines.Add(clean);
        }
        
        return string.Join(" ", textLines.Distinct());
    }
}