using Hangfire;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Runs one lead's "areas of improvement" video analysis on a worker.
/// The transcript fetch (yt-dlp, 45s cap) plus the vision call each get
/// minutes, so this never belonged inside a request. The result is
/// persisted as a Blocker note so the details page shows it after reload.
/// </summary>
public class VideoReviewJob
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly YouTubeTranscriptService _transcript;
    private readonly ILogger<VideoReviewJob> _log;

    public VideoReviewJob(
        DatabaseService db,
        AIService ai,
        YouTubeTranscriptService transcript,
        ILogger<VideoReviewJob> log)
    {
        _db = db;
        _ai = ai;
        _transcript = transcript;
        _log = log;
    }

    [AutomaticRetry(Attempts = 0)]
    [Queue(BackgroundQueues.Ai)]
    public async Task RunAsync(string jobId, int contactId)
    {
        var stop = JobCancellation.Register(jobId);
        JobTrackerStore.Update(jobId, j =>
        {
            j.State = JobState.Running;
            j.StartedAt = DateTime.UtcNow;
            j.Total = 1;
            j.Message = "Fetching transcript...";
        });

        try
        {
            var contact = await _db.GetContactAsync(contactId);
            if (contact is null)
            {
                JobTrackerStore.Update(jobId, j =>
                {
                    j.State = JobState.Failed;
                    j.FinishedAt = DateTime.UtcNow;
                    j.Message = "Lead no longer exists.";
                });
                return;
            }

            string transcript = string.Empty;
            if (!string.IsNullOrWhiteSpace(contact.VideoUrl))
            {
                try
                {
                    var cues = await _transcript.ExtractTranscriptCuesAsync(contact.VideoUrl);
                    transcript = string.Join(" ", cues.Select(c => c.Text).Distinct());
                }
                catch
                {
                    // No transcript is survivable: snapshots + metadata still work.
                }
            }

            if (stop.IsCancellationRequested)
            {
                JobTrackerStore.Update(jobId, j =>
                {
                    j.State = JobState.Cancelled;
                    j.FinishedAt = DateTime.UtcNow;
                    j.Message = "Cancelled.";
                });
                return;
            }

            JobTrackerStore.Heartbeat(jobId, "Asking the AI for editing notes...");
            var advice = await _ai.GenerateVideoImprovementsAsync(contact, transcript, contact.VideoSnapshot);

            if (string.IsNullOrWhiteSpace(advice))
            {
                JobTrackerStore.Update(jobId, j =>
                {
                    j.State = JobState.Failed;
                    j.FinishedAt = DateTime.UtcNow;
                    j.Processed = 1;
                    j.Message = "The AI service could not produce an analysis. It may have timed out - try again.";
                });
                return;
            }

            VideoReviewStore.Save(contact.Id, advice);

            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Completed;
                j.FinishedAt = DateTime.UtcNow;
                j.Processed = 1;
                j.Succeeded = 1;
                j.Message = "Analysis ready.";
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Video review {JobId} failed.", jobId);
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Failed;
                j.FinishedAt = DateTime.UtcNow;
                j.Message = "Analysis failed: " + ex.Message;
            });
        }
        finally
        {
            JobCancellation.Unregister(jobId);
        }
    }
}
