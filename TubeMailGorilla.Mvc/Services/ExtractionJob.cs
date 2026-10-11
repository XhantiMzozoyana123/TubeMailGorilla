using Hangfire;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Runs one keyword extraction on a Hangfire background worker.
///
/// Resilience: never throws (failures land in the JobRecord), checks
/// cooperative cancellation between videos via progress callbacks, and
/// heartbeats so a stalled run is visible instead of silent.
/// </summary>
public class ExtractionJob
{
    private readonly ExtractService _extract;
    private readonly ILogger<ExtractionJob> _log;

    public ExtractionJob(ExtractService extract, ILogger<ExtractionJob> log)
    {
        _extract = extract;
        _log = log;
    }

    [AutomaticRetry(Attempts = 0)]
    [Queue(BackgroundQueues.Extraction)]
    public async Task RunAsync(string jobId, string keyword, int pageViewLimit,
        bool gmailOnly, bool validateEmails)
    {
        var stop = JobCancellation.Register(jobId);
        var started = DateTime.UtcNow;
        JobTrackerStore.Update(jobId, j =>
        {
            j.State = JobState.Running;
            j.StartedAt = started;
            j.Message = "Searching YouTube...";
        });

        try
        {
            var progress = new Progress<int>(pct => JobTrackerStore.Heartbeat(
                jobId, $"Working... {pct}%", null));

            var result = await _extract.ExtractByKeywordAsync(
                keyword, pageViewLimit, gmailOnly, validateEmails,
                progress, stop);

            if (stop.IsCancellationRequested)
            {
                JobTrackerStore.Update(jobId, j =>
                {
                    j.State = JobState.Cancelled;
                    j.FinishedAt = DateTime.UtcNow;
                    j.Message = "Cancelled. Partial results are kept.";
                });
                return;
            }

            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Completed;
                j.FinishedAt = DateTime.UtcNow;
                j.Total = result.TotalVideos;
                j.Processed = result.TotalVideos;
                j.Succeeded = result.EmailsFound;
                j.Failed = result.Errors;
                j.Message = $"Done. Videos: {result.TotalVideos}, leads: {result.EmailsFound}, errors: {result.Errors}.";
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Extraction job {JobId} for '{Keyword}' failed.", jobId, keyword);
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Failed;
                j.FinishedAt = DateTime.UtcNow;
                j.Message = "Extraction failed: " + ex.Message;
            });
        }
        finally
        {
            JobCancellation.Unregister(jobId);
        }
    }
}
