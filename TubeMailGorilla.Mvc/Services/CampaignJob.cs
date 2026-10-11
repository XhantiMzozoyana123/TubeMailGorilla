using Hangfire;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Sends a whole campaign on a Hangfire worker: per-recipient try/catch,
/// 500ms pacing between sends, cooperative cancellation, live counters.
/// One recipient failing (bad address, SMTP hiccup) never stops the rest.
/// </summary>
public partial class CampaignJob
{
    private readonly DatabaseService _db;
    private readonly EmailService _email;
    private readonly AIService _ai;
    private readonly ILogger<CampaignJob> _log;

    public CampaignJob(DatabaseService db, EmailService email, AIService ai, ILogger<CampaignJob> log)
    {
        _db = db;
        _email = email;
        _ai = ai;
        _log = log;
    }

    [AutomaticRetry(Attempts = 0)]
    [Queue(BackgroundQueues.Campaign)]
    public async Task RunAsync(string jobId, CampaignRequest request)
    {
        var stop = JobCancellation.Register(jobId);
        var started = DateTime.UtcNow;
        var sent = 0;
        var failed = 0;
        var skipped = 0;

        try
        {
            var contacts = new List<Models.EmailContact>();
            foreach (var id in request.ContactIds)
            {
                var contact = await _db.GetContactAsync(id);
                if (contact is not null) contacts.Add(contact);
            }

            var senders = await _db.GetSendersAsync();
            var parameters = await _db.GetMessageParametersAsync();

            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Running;
                j.StartedAt = started;
                j.Total = contacts.Count;
                j.Message = contacts.Count == 0
                    ? "No recipients resolved."
                    : $"Sending to {contacts.Count} leads...";
            });

            if (contacts.Count == 0 || senders.Count == 0)
            {
                JobTrackerStore.Update(jobId, j =>
                {
                    j.State = JobState.Failed;
                    j.FinishedAt = DateTime.UtcNow;
                    j.Message = senders.Count == 0
                        ? "No active email accounts. Add one in Settings."
                        : "No contacts to email.";
                });
                return;
            }

            var openers = await _db.GetOpenersAsync();
            var latestOpenerByContact = openers
                .Where(o => o.EmailerId > 0 && !string.IsNullOrWhiteSpace(o.Text))
                .GroupBy(o => o.EmailerId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAt).First().Text.Trim());

            var variations = request.Variations
                .Where(v => !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body))
                .ToList();
            var rotateMessages = request.AllowMessageRotation && variations.Count > 0;

            for (var i = 0; i < contacts.Count; i++)
            {
                if (stop.IsCancellationRequested) break;
                var contact = contacts[i];
                JobTrackerStore.Heartbeat(jobId, $"Sending {i + 1}/{contacts.Count} ({contact.Email})", i);

                var outcome = await SendOneAsync(
                    contact, i, request, senders, parameters,
                    latestOpenerByContact, variations, rotateMessages, stop);
                if (outcome == SendOutcome.Sent) sent++;
                else if (outcome == SendOutcome.Failed) failed++;
                else skipped++;

                JobTrackerStore.Update(jobId, j =>
                {
                    j.Processed = i + 1;
                    j.Succeeded = sent;
                    j.Failed = failed;
                    j.Skipped = skipped;
                });
            }

            var cancelled = stop.IsCancellationRequested;
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = cancelled ? JobState.Cancelled : JobState.Completed;
                j.FinishedAt = DateTime.UtcNow;
                j.Message = cancelled
                    ? $"Cancelled. Sent: {sent}, failed: {failed}, skipped: {skipped}."
                    : $"All done. Sent: {sent}, failed: {failed}, skipped: {skipped}.";
            });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Campaign job {JobId} failed.", jobId);
            JobTrackerStore.Update(jobId, j =>
            {
                j.State = JobState.Failed;
                j.FinishedAt = DateTime.UtcNow;
                j.Message = "Send failed: " + ex.Message;
            });
        }
        finally
        {
            JobCancellation.Unregister(jobId);
        }
    }
}
