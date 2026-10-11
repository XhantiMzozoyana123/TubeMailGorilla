using Hangfire;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Well-known Hangfire queue names, one per workload so a stuck extraction
/// can never starve a campaign send behind it.
/// </summary>
public static class BackgroundQueues
{
    public const string Extraction = "extraction";
    public const string Bulk = "bulk";
    public const string Campaign = "campaign";
    public const string Ai = "ai";

    public static string[] All => new[] { Extraction, Bulk, Campaign, Ai };

    public static string For(JobKind kind) => kind switch
    {
        JobKind.BulkExtraction => Bulk,
        JobKind.Campaign => Campaign,
        JobKind.Icebreakers => Ai,
        JobKind.VideoReview => Ai,
        _ => Extraction
    };
}

/// <summary>
/// Thin enqueue façade: creates the UI-facing JobRecord first (so the page
/// has something to poll instantly), then hands the work to Hangfire.
/// Controllers never touch IBackgroundJobClient directly.
/// </summary>
public class BackgroundJobService
{
    private readonly IBackgroundJobClient _client;
    private readonly ILogger<BackgroundJobService> _log;

    public BackgroundJobService(IBackgroundJobClient client, ILogger<BackgroundJobService> log)
    {
        _client = client;
        _log = log;
    }

    public string EnqueueExtraction(string keyword, int pageLimit, bool gmailOnly, bool validateEmails)
    {
        var record = JobTrackerStore.Create(JobKind.Extraction, keyword);
        _client.Enqueue<ExtractionJob>(
            job => job.RunAsync(record.JobId, keyword, pageLimit, gmailOnly, validateEmails));
        _log.LogInformation("Queued extraction {JobId} for '{Keyword}'.", record.JobId, keyword);
        return record.JobId;
    }

    public string EnqueueBulkExtraction(List<(string Keyword, int Pages)> rows, bool gmailOnly, bool validateEmails)
    {
        var record = JobTrackerStore.Create(JobKind.BulkExtraction, $"{rows.Count} keywords");
        var keywords = rows.Select(r => r.Keyword).ToList();
        var pages = rows.Select(r => r.Pages).ToList();
        _client.Enqueue<BulkExtractionJob>(
            job => job.RunAsync(record.JobId, keywords, pages, gmailOnly, validateEmails));
        _log.LogInformation("Queued bulk extraction {JobId} ({Count} keywords).", record.JobId, rows.Count);
        return record.JobId;
    }

    public string EnqueueCampaign(CampaignRequest request)
    {
        var record = JobTrackerStore.Create(JobKind.Campaign, request.Subject);
        _client.Enqueue<CampaignJob>(job => job.RunAsync(record.JobId, request));
        _log.LogInformation("Queued campaign {JobId} '{Subject}'.", record.JobId, request.Subject);
        return record.JobId;
    }

    public string EnqueueIcebreakers(List<int> contactIds, string query)
    {
        var record = JobTrackerStore.Create(JobKind.Icebreakers, $"{contactIds.Count} leads");
        _client.Enqueue<IcebreakerBatchJob>(job => job.RunAsync(record.JobId, contactIds, query));
        _log.LogInformation("Queued icebreakers {JobId} ({Count} leads).", record.JobId, contactIds.Count);
        return record.JobId;
    }

    public string EnqueueVideoReview(int contactId, string email)
    {
        var record = JobTrackerStore.Create(JobKind.VideoReview, email);
        _client.Enqueue<VideoReviewJob>(job => job.RunAsync(record.JobId, contactId));
        _log.LogInformation("Queued video review {JobId} for contact {ContactId}.", record.JobId, contactId);
        return record.JobId;
    }
}
