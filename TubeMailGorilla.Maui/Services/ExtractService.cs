using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

public class ExtractService
{
    private readonly DatabaseService _db;
    private readonly YouTubeExplodeService _youtube;
    private readonly AIService _ai;
    private readonly EmailService _email;

    public ExtractService(DatabaseService db, YouTubeExplodeService youtube, AIService ai, EmailService email)
    {
        _db = db;
        _youtube = youtube;
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
            // Search YouTube for videos matching the keyword (YoutubeExplode).
            // Bounded so a stalled search can never hang the extraction start.
            var videos = await WithTimeout(
                _youtube.SearchAsync(keyword, pageViewLimit),
                TimeSpan.FromSeconds(60),
                new List<YouTubeVideo>());
            result.TotalVideos = videos.Count;

            for (int i = 0; i < videos.Count; i++)
            {
                var video = videos[i];
                try
                {
                    // Get description (from the video's watch page metadata). Bounded
                    // so a stalled HTTP request can never hang the extraction loop.
                    string description = await WithTimeout(
                        _youtube.GetDescriptionAsync(video.Url),
                        TimeSpan.FromSeconds(45),
                        string.Empty);

                    // Get captions/transcript (closed captions via YoutubeExplode).
                    // Also bounded - a video with no/hanging captions is skipped data-wise.
                    string subtitles = await WithTimeout(
                        _youtube.GetTranscriptAsync(video.Url),
                        TimeSpan.FromSeconds(45),
                        string.Empty);

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

                    // Build the lead from the regex-extracted data...
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

                    // ...then enrich it with AI fields under a hard per-video budget,
                    // so a slow first-run model download or CPU inference can never
                    // stall the whole extraction on one video (fields are left empty).
                    var aiTask = _ai.ExtractAllAsync(emailer);
                    if (await Task.WhenAny(aiTask, Task.Delay(TimeSpan.FromSeconds(90))) != aiTask)
                    {
                        // Budget exceeded - clear AI fields; the late-finishing task
                        // writes empty strings, never partial data.
                        emailer.FullName = string.Empty;
                        emailer.Company = string.Empty;
                        emailer.Job = string.Empty;
                        emailer.Location = string.Empty;
                        emailer.Industry = string.Empty;
                    }

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
            string description = await _youtube.GetDescriptionAsync(videoUrl);

            string subtitles = await _youtube.GetTranscriptAsync(videoUrl);

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

    /// <summary>
    /// Awaits a task with a hard timeout, returning <paramref name="fallback"/>
    /// when the task doesn't complete in time. Used to bound every network and
    /// inference step so the extraction loop can never hang on one video.
    /// </summary>
    private static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeout, T fallback)
    {
        if (await Task.WhenAny(task, Task.Delay(timeout)) != task)
            return fallback;
        try { return await task; }
        catch { return fallback; }
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