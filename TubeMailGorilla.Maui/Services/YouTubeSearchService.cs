using System.Diagnostics;
using System.Text;

namespace TubeMailGorilla.Maui.Services;

public class YouTubeSearchService
{
    /// <summary>
    /// Resolves up to <paramref name="maxResults"/> YouTube videos for a query by
    /// invoking the bundled yt-dlp binary (flat-playlist search). Unlike scraping
    /// the HTML search page, this is reliable because yt-dlp handles YouTube's
    /// cookies/consent and returns stable video IDs, titles and channel names.
    /// </summary>
    public async Task<List<YouTubeVideo>> SearchAsync(string query, int maxResults = 10)
    {
        var videos = new List<YouTubeVideo>();
        try
        {
            var ytDlpPath = await YtDlp.GetPathAsync();
            var tempDir = Path.Combine(Path.GetTempPath(), "ytsearch_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var psi = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                WorkingDirectory = tempDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };

            // "ytsearchN:<query>" resolves to the first N search results.
            psi.ArgumentList.Add($"ytsearch{Math.Max(1, maxResults)}:{query}");
            psi.ArgumentList.Add("--flat-playlist");
            psi.ArgumentList.Add("--skip-download");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--print");
            psi.ArgumentList.Add("%(id)s\t%(title)s\t%(channel)s");

            using var proc = new Process { StartInfo = psi };
            proc.Start();
            var stdout = await proc.StandardOutput.ReadToEndAsync();
            await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var rawLine in stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = rawLine.Split('\t');
                var id = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                if (id.Length != 11 || !seen.Add(id))
                    continue;

                videos.Add(new YouTubeVideo
                {
                    VideoId = id,
                    Url = $"https://www.youtube.com/watch?v={id}",
                    Title = parts.Length > 1 ? parts[1].Trim() : $"Video {id}",
                    Author = parts.Length > 2 ? parts[2].Trim() : null
                });

                if (videos.Count >= maxResults)
                    break;
            }

            try { Directory.Delete(tempDir, true); } catch { }
        }
        catch
        {
            // Search failed entirely; caller treats an empty list as "no results".
        }

        return videos;
    }
}

public class YouTubeVideo
{
    public string VideoId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public long? ViewCount { get; set; }
}