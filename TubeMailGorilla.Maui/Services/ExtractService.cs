using System.Diagnostics;
using System.Text;
using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

public class ExtractService
{
    private readonly DatabaseService _db;
    private readonly YouTubeSearchService _ytSearch;
    private readonly YouTubeTranscriptService _transcript;
    private readonly AIService _ai;
    private readonly EmailService _email;

    public ExtractService(DatabaseService db, YouTubeSearchService ytSearch, YouTubeTranscriptService transcript, AIService ai, EmailService email)
    {
        _db = db;
        _ytSearch = ytSearch;
        _transcript = transcript;
        _ai = ai;
        _email = email;
    }

    /// <summary>
    /// Extract leads by searching YouTube for the given keyword.
    /// Mirrors the backend ExtractService.initialExtractionAsync.
    /// </summary>
    public async Task<ExtractionResult> ExtractByKeywordAsync(
        string keyword,
        int pageViewLimit,
        bool gmailAccountOnly,
        bool validateEmails,
        IProgress<int>? progress = null)
    {
        var result = new ExtractionResult { TotalVideos = 0, EmailsFound = 0, Errors = 0 };

        try
        {
            // Search YouTube for videos matching the keyword
            var videos = await _ytSearch.SearchAsync(keyword, pageViewLimit);
            result.TotalVideos = videos.Count;

            for (int i = 0; i < videos.Count; i++)
            {
                var video = videos[i];
                try
                {
                    // Get description
                    string description = await GetYouTubeVideoDescription(video.Url);

                    // Get captions
                    string subtitles = string.Empty;
                    try
                    {
                        subtitles = await _transcript.ExtractTranscriptAsync(video.Url);
                    }
                    catch { }

                    // Extract emails
                    var emailFound = _email.ExtractEmails(description + " " + subtitles);
                    if (string.IsNullOrEmpty(emailFound))
                    {
                        result.Errors++;
                        progress?.Report((i + 1) * 100 / videos.Count);
                        continue;
                    }

                    // Gmail accounts only filter
                    if (gmailAccountOnly && !emailFound.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Errors++;
                        progress?.Report((i + 1) * 100 / videos.Count);
                        continue;
                    }

                    // Validate emails filter
                    if (validateEmails && !await _email.ValidateEmailAsync(emailFound))
                    {
                        result.Errors++;
                        progress?.Report((i + 1) * 100 / videos.Count);
                        continue;
                    }

                    // Extract phone
                    var phoneFound = _email.ExtractPhoneNumbers(description + " " + subtitles);

                    // Extract AI info
                    var emailer = new Emailer
                    {
                        VideoTitle = video.Title,
                        VideoDescription = description,
                        VideoUrl = video.Url,
                        VideoTranscript = subtitles,
                        SearchTerm = keyword,
                        Email = emailFound,
                        Phone = phoneFound,
                        Channel = video.Author ?? string.Empty,
                        Status = EmailerStatus.New.ToString()
                    };

                    try
                    {
                        await _ai.ExtractAllAsync(emailer);
                    }
                    catch { }

                    // Save to database
                    var contact = new EmailContact
                    {
                        Email = emailer.Email,
                        Name = NormalizeName(emailer.FullName),
                        Channel = emailer.Channel,
                        VideoTitle = emailer.VideoTitle,
                        VideoDescription = emailer.VideoDescription,
                        ExtractedAt = DateTime.Now
                    };

                    await _db.AddContactAsync(contact);
                    result.EmailsFound++;
                }
                catch
                {
                    result.Errors++;
                }

                progress?.Report((i + 1) * 100 / videos.Count);
            }
        }
        catch
        {
            // Search failed entirely
        }

        return result;
    }

    /// <summary>Legacy single-video extraction (kept for backward compatibility).</summary>
    public async Task<int> ExtractFromVideoAsync(string videoUrl, string keyword)
    {
        try
        {
            string description = await GetYouTubeVideoDescription(videoUrl);

            string subtitles = string.Empty;
            try
            {
                subtitles = await _transcript.ExtractTranscriptAsync(videoUrl);
            }
            catch { }

            var emailFound = _email.ExtractEmails(description + " " + subtitles);
            if (string.IsNullOrEmpty(emailFound))
                return 0;

            var phoneFound = _email.ExtractPhoneNumbers(description + " " + subtitles);

            var emailer = new Emailer
            {
                VideoTitle = ExtractTitleFromUrl(videoUrl),
                VideoDescription = description,
                VideoUrl = videoUrl,
                VideoTranscript = subtitles,
                SearchTerm = keyword,
                Email = emailFound,
                Phone = phoneFound,
                Status = EmailerStatus.New.ToString()
            };

            await _ai.ExtractAllAsync(emailer);

            var contact = new EmailContact
            {
                Email = emailer.Email,
                Name = NormalizeName(emailer.FullName),
                Channel = emailer.Channel,
                VideoTitle = emailer.VideoTitle,
                VideoDescription = emailer.VideoDescription,
                ExtractedAt = DateTime.Now
            };

            await _db.AddContactAsync(contact);
            return 1;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<int> BatchExtractAsync(List<string> videoUrls, string keyword, IProgress<int>? progress = null)
    {
        var totalExtracted = 0;
        for (int i = 0; i < videoUrls.Count; i++)
        {
            var count = await ExtractFromVideoAsync(videoUrls[i], keyword);
            totalExtracted += count;
            progress?.Report((i + 1) * 100 / videoUrls.Count);
        }
        return totalExtracted;
    }

    private async Task<string> GetYouTubeVideoDescription(string videoUrl)
    {
        try
        {
            var ytDlpPath = await YtDlp.GetPathAsync();

            var psi = new ProcessStartInfo
            {
                FileName = ytDlpPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
            };

            psi.ArgumentList.Add("--skip-download");
            psi.ArgumentList.Add("--no-warnings");
            psi.ArgumentList.Add("--get-description");
            psi.ArgumentList.Add(videoUrl);

            using var proc = new Process { StartInfo = psi };
            proc.Start();
            var descTask = proc.StandardOutput.ReadToEndAsync();

            // Hard timeout: if yt-dlp hangs (throttling, outdated binary, network
            // stall), kill it and continue rather than freezing the extraction.
            var waitTask = proc.WaitForExitAsync();
            var winner = await Task.WhenAny(waitTask, Task.Delay(TimeSpan.FromSeconds(45)));
            if (winner != waitTask)
            {
                try { proc.Kill(true); } catch { }
                return string.Empty;
            }

            var desc = await descTask;
            await proc.StandardError.ReadToEndAsync();

            return (desc ?? string.Empty).Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractTitleFromUrl(string videoUrl)
    {
        try
        {
            var uri = new Uri(videoUrl);
            return uri.Query.Split('&').FirstOrDefault(q => q.StartsWith("v="))?.Substring(2) ?? "Unknown Video";
        }
        catch
        {
            return "Unknown Video";
        }
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var trimmed = name.Trim();
        if (trimmed.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return trimmed;
    }
}

/// <summary>
/// Contains the summary of a keyword-based extraction run.
/// </summary>
public class ExtractionResult
{
    public int TotalVideos { get; set; }
    public int EmailsFound { get; set; }
    public int Errors { get; set; }
}