using Hangfire;
using Hangfire.Annotations;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using TubeMailGorilla.Mvc.Models;
namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Enqueues extraction work onto the Hangfire background server and manages the
/// recurring schedules.
/// </summary>
public class ExtractionJobScheduler
{
    /// <summary>Recurring job id - one per schedule we support.</summary>
    private const string DailyCronJobId = "extraction-daily-cron";
    private const string HourlyCronJobId = "extraction-hourly-cron";

    /// <summary>Queue name for extraction jobs - lets the dashboard filter them.</summary>
    public const string QueueName = "extraction";

    private readonly IBackgroundJobClient _client;
    private readonly IRecurringJobManager _recurring;
    private readonly ILogger<ExtractionJobScheduler> _log;

    public ExtractionJobScheduler(
        IBackgroundJobClient client,
        IRecurringJobManager recurring,
        ILogger<ExtractionJobScheduler> log)
    {
        _client = client;
        _recurring = recurring;
        _log = log;
    }

    /// <summary>
    /// Queues a one-off extraction and returns the job id so the caller can
    /// poll its status.
    /// </summary>
    public string Enqueue(string keyword, int pageViewLimit, bool gmailOnly, bool validateEmails)
    {
        // Create<T>(expression, state) is the only typed overload, and its state
        // argument is REQUIRED - Hangfire dereferences it while building the job,
        // so passing null throws "Value cannot be null. (Parameter 'state')" at
        // enqueue time. CreateEnqueuedState is the public factory for the state
        // Hangfire itself would create; "Enqueued" is a job-parameter value and
        // must go in Data, not in the reason.
        var initialState = new EnqueuedState(QueueName);

        var jobId = _client.Create<ExtractionJobRunner>(
            runner => runner.RunAsync(keyword, pageViewLimit, gmailOnly, validateEmails),
            initialState);

        // Seed the record immediately so the page has something to show while
        // the job is still sitting in the queue.
        ExtractionRunStore.Save(new ExtractionRunStatus
        {
            JobId = jobId!,
            Keyword = keyword,
            State = "Queued",
            Message = "Queued - waiting for a background worker."
        });

        _log.LogInformation("Queued extraction job {JobId} for '{Keyword}'.", jobId, keyword);
        return jobId!;
    }

    /// <summary>Schedules (or removes) a daily extraction at the given local time.</summary>
    public void SetDailySchedule(string? timeOfDay, string keyword, int pageViewLimit)
    {
        Remove(DailyCronJobId);

        if (string.IsNullOrWhiteSpace(timeOfDay) || string.IsNullOrWhiteSpace(keyword))
        {
            WebPreferences.Set("ExtractCronDailyTime", string.Empty);
            return;
        }

        if (!TimeSpan.TryParse(timeOfDay, out var at))
        {
            _log.LogWarning("Ignoring daily schedule: '{TimeOfDay}' is not a valid time.", timeOfDay);
            return;
        }

        // Cron uses a server-wide clock; the VPS is on UTC, so the admin-facing
        // time is treated as UTC and stated as such in the UI.
        var cron = $"0 {at.Minutes} {at.Hours} * * *";

        _recurring.AddOrUpdate<ExtractionJobRunner>(DailyCronJobId,
            runner => runner.RunAsync(keyword, pageViewLimit, false, false),
            cron,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        WebPreferences.Set("ExtractCronDailyTime", timeOfDay);

        _log.LogInformation("Daily extraction scheduled at {Time:HH:mm} UTC for '{Keyword}'.", at, keyword);
    }

    /// <summary>Schedules (or removes) an hourly extraction.</summary>
    public void SetHourlySchedule(bool enabled, string keyword, int pageViewLimit)
    {
        Remove(HourlyCronJobId);

        WebPreferences.Set("ExtractCronHourly", false);

        if (!enabled || string.IsNullOrWhiteSpace(keyword))
            return;

        // Every 5 minutes, not every minute: each run is tens of minutes of
        // video download and inference, so a 1-minute cron would pile up
        // overlapping runs behind a single worker.
        _recurring.AddOrUpdate<ExtractionJobRunner>(HourlyCronJobId,
            runner => runner.RunAsync(keyword, pageViewLimit, false, false),
            "*/5 * * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        WebPreferences.Set("ExtractCronHourly", true);

        _log.LogInformation("Hourly extraction scheduled every 5 minutes for '{Keyword}'.", keyword);
    }

    /// <summary>Current schedule state, for rendering the Settings page.</summary>
    // Schedule state is tracked in WebPreferences rather than queried back out of
    // Hangfire's storage: IRecurringJobManager has no read API (only AddOrUpdate /
    // RemoveIfExists / Trigger), and the dashboard remains the source of truth for
    // what Hangfire actually holds.
    public (bool Daily, string? DailyTime) GetDailySchedule()
    {
        var time = WebPreferences.Get("ExtractCronDailyTime", string.Empty);
        return (time.Length > 0, time.Length > 0 ? time : null);
    }

    public bool GetHourlySchedule() =>
        WebPreferences.Get("ExtractCronHourly", false);

    private void Remove(string jobId) => _recurring.RemoveIfExists(jobId);
}
