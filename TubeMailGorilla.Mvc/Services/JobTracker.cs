namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Kind of background work a tracked job performs. One Hangfire queue per
/// kind, so a stuck extraction can never starve a campaign send behind it.
/// </summary>
public enum JobKind
{
    Extraction,
    BulkExtraction,
    Campaign,
    Icebreakers,
    VideoReview
}

/// <summary>
/// Lifecycle of a tracked job. Queued = accepted but no worker picked it up;
/// Running = a worker owns it; Completed/Failed/Cancelled are terminal.
/// </summary>
public enum JobState
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// One background job's durable status record. Written by the worker,
/// read by the pages that poll /jobs/{id}.
/// </summary>
public sealed class JobRecord
{
    public string JobId { get; set; } = string.Empty;
    public JobKind Kind { get; set; }
    public JobState State { get; set; } = JobState.Queued;

    /// <summary>Human label, e.g. keyword, campaign subject, "12 leads".</summary>
    public string Label { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public int Total { get; set; }
    public int Processed { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime LastHeartbeatUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive => State is JobState.Queued or JobState.Running;

    public string ElapsedLabel
    {
        get
        {
            var end = FinishedAt ?? DateTime.UtcNow;
            if (StartedAt is null) return string.Empty;
            var span = end - StartedAt.Value;
            if (span < TimeSpan.Zero) span = TimeSpan.Zero;
            return span.TotalHours >= 1
                ? $"{(int)span.TotalHours}h {span.Minutes}m"
                : $"{(int)span.TotalMinutes}m {span.Seconds}s";
        }
    }

    public double ProgressFraction =>
        Total <= 0 ? 0 : Math.Clamp(Processed / (double)Total, 0, 1);
}
