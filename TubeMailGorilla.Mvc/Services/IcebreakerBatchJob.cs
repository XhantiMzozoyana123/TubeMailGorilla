using Hangfire;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Bulk-generates one icebreaker per lead on a worker. Per-lead try/catch
/// with the 3-consecutive-failure circuit breaker from the desktop page:
/// a dead AI server stops the batch instead of burning a 300s timeout
/// on every remaining lead.
/// </summary>
public class IcebreakerBatchJob
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly ILogger<IcebreakerBatchJob> _log;

    public IcebreakerBatchJob(DatabaseService db, AIService ai, ILogger<IcebreakerBatchJob> log)
    {
        _db = db;
        _ai = ai;
        _log = log;
    }

    [AutomaticRetry(Attempts = 0)]
    [Queue(BackgroundQueues.Ai)]
    public async Task RunAsync(string jobId, List<int> contactIds, string query)
    {
        var stop = JobCancellation.Register(jobId);
        var created = 0;
        var failed = 0;
        var consecutiveFailures = 0;

        JobTrackerStore.Update(jobId, j =>
        {
            j.State = JobState.Running;
            j.StartedAt = DateTime.UtcNow;
            j.Total = contactIds.Count;
            j.Message = $"Generating icebreakers for {contactIds.Count} leads...";
        });

        try
        {
            for (var i = 0; i < contactIds.Count; i++)
            {
                if (stop.IsCancellationRequested) break;

                var contact = await _db.GetContactAsync(contactIds[i]);
                if (contact is null)
                {
                    failed++;
                    continue;
                }

                JobTrackerStore.Heartbeat(jobId, $"Lead {i + 1}/{contactIds.Count}: {contact.Email}", i);

                try
                {
                    var icebreaker = await _ai.GenerateIcebreakerAsync(contact);
                    if (string.IsNullOrWhiteSpace(icebreaker))
                    {
                        failed++;
                        if (++consecutiveFailures >= 3) break;
                        continue;
                    }

                    consecutiveFailures = 0;
                    await _db.SaveOpenerAsync(new Models.Opener
                    {
                        EmailerId = contact.Id,
                        Text = icebreaker,
                        CreatedAt = DateTime.Now
                    });
                    created++;
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "Icebreaker for contact {Id} failed.", contact.Id);
                    failed++;
                    if (++consecutiveFailures >= 3) break;
                }

                JobTrackerStore.Update(jobId, j =>
                {
                    j.Processed = i + 1;
                    j.Succeeded = created;
                    j.Failed = failed;
                });
            }

            var cancelled = stop.IsCancellationRequested;
            var circuitOpen = consecutiveFailures >= 3;
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = cancelled ? JobState.Cancelled : JobState.Completed;
                j.FinishedAt = DateTime.UtcNow;
                var summary = $"Created {created} icebreaker{(created == 1 ? "" : "s")}";
                if (failed > 0) summary += $", {failed} failed (AI timed out or unreachable).";
                if (circuitOpen) summary += " Stopped early after 3 consecutive failures.";
                if (cancelled) summary += " Cancelled.";
                j.Message = summary;
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Icebreaker batch {JobId} failed.", jobId);
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Failed;
                j.FinishedAt = DateTime.UtcNow;
                j.Message = "Icebreaker run failed: " + ex.Message;
            });
        }
        finally
        {
            JobCancellation.Unregister(jobId);
        }
    }
}
