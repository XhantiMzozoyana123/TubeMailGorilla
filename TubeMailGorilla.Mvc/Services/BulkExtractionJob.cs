using Hangfire;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Runs a multi-keyword bulk extraction as ONE background job. Each keyword
/// is a checkpoint: per-keyword try/catch means one bad keyword cannot kill
/// the rest, and the counters show exactly how far it got.
/// </summary>
public class BulkExtractionJob
{
    private readonly ExtractService _extract;
    private readonly ILogger<BulkExtractionJob> _log;

    public BulkExtractionJob(ExtractService extract, ILogger<BulkExtractionJob> log)
    {
        _extract = extract;
        _log = log;
    }

    [AutomaticRetry(Attempts = 0)]
    [Queue(BackgroundQueues.Bulk)]
    public async Task RunAsync(string jobId, List<string> keywords, List<int> pages,
        bool gmailOnly, bool validateEmails)
    {
        var stop = JobCancellation.Register(jobId);
        var started = DateTime.UtcNow;
        JobTrackerStore.Update(jobId, j =>
        {
            j.State = JobState.Running;
            j.StartedAt = started;
            j.Total = keywords.Count;
            j.Message = $"Starting bulk run: {keywords.Count} keywords.";
        });

        var totalVideos = 0;
        var totalEmails = 0;
        var totalErrors = 0;

        try
        {
            for (var i = 0; i < keywords.Count; i++)
            {
                if (stop.IsCancellationRequested) break;

                var keyword = keywords[i];
                var pageLimit = i < pages.Count && pages[i] > 0 ? pages[i] : 5;
                JobTrackerStore.Heartbeat(jobId, $"Keyword {i + 1}/{keywords.Count}: {keyword}", i);

                try
                {
                    var result = await _extract.ExtractByKeywordAsync(
                        keyword, pageLimit, gmailOnly, validateEmails, null, stop);
                    totalVideos += result.TotalVideos;
                    totalEmails += result.EmailsFound;
                    totalErrors += result.Errors;
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Bulk keyword '{Keyword}' failed; continuing.", keyword);
                    totalErrors++;
                }

                JobTrackerStore.Update(jobId, j =>
                {
                    j.Processed = i + 1;
                    j.Succeeded = totalEmails;
                    j.Failed = totalErrors;
                });
            }

            var cancelled = stop.IsCancellationRequested;
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = cancelled ? JobState.Cancelled : JobState.Completed;
                j.FinishedAt = DateTime.UtcNow;
                j.Processed = cancelled ? j.Processed : keywords.Count;
                j.Message = cancelled
                    ? $"Cancelled after {j.Processed}/{keywords.Count} keywords. Videos: {totalVideos}, leads: {totalEmails}."
                    : $"Bulk complete. Keywords: {keywords.Count}, videos: {totalVideos}, leads: {totalEmails}, errors: {totalErrors}.";
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Bulk extraction job {JobId} failed.", jobId);
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Failed;
                j.FinishedAt = DateTime.UtcNow;
                j.Message = "Bulk extraction failed: " + ex.Message;
            });
        }
        finally
        {
            JobCancellation.Unregister(jobId);
        }
    }
}
