namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Process-wide durable store of job records. The dashboard holds the raw
/// Hangfire history; this is the UI-facing projection (progress counters,
/// friendly messages). Bounded so a long-lived server cannot grow it.
/// </summary>
public static class JobTrackerStore
{
    private const int MaxRecords = 100;
    private static readonly Dictionary<string, JobRecord> Records = new(StringComparer.Ordinal);
    private static readonly object Gate = new();

    public static JobRecord Create(JobKind kind, string label)
    {
        var record = new JobRecord
        {
            JobId = Guid.NewGuid().ToString("N"),
            Kind = kind,
            Label = label,
            State = JobState.Queued,
            Message = "Queued - waiting for a background worker."
        };
        lock (Gate)
        {
            Records[record.JobId] = record;
            TrimLocked();
        }
        return record;
    }

    public static void Update(string jobId, Action<JobRecord> mutate)
    {
        lock (Gate)
        {
            if (Records.TryGetValue(jobId, out var record))
                mutate(record);
        }
    }

    public static void Heartbeat(string jobId, string? message = null, int? processed = null)
    {
        Update(jobId, r =>
        {
            r.LastHeartbeatUtc = DateTime.UtcNow;
            if (message is not null) r.Message = message;
            if (processed.HasValue) r.Processed = processed.Value;
        });
    }

    public static JobRecord? Get(string? jobId)
    {
        lock (Gate)
        {
            if (!string.IsNullOrEmpty(jobId) && Records.TryGetValue(jobId, out var record))
                return record;
            return null;
        }
    }

    public static List<JobRecord> Recent(JobKind? kind = null, int take = 10)
    {
        lock (Gate)
        {
            return Records.Values
                .Where(r => kind is null || r.Kind == kind)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToList();
        }
    }

    public static JobRecord? LatestActive(JobKind? kind = null)
    {
        lock (Gate)
        {
            return Records.Values
                .Where(r => r.IsActive && (kind is null || r.Kind == kind))
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// True when a job of this kind is already queued/running. Single-flight
    /// guard: one campaign at a time, one bulk run at a time.
    /// </summary>
    public static bool HasActive(JobKind kind)
    {
        lock (Gate)
        {
            return Records.Values.Any(r => r.Kind == kind && r.IsActive);
        }
    }

    private static void TrimLocked()
    {
        if (Records.Count <= MaxRecords) return;
        foreach (var key in Records.Values
            .Where(r => !r.IsActive)
            .OrderBy(r => r.CreatedAt)
            .Take(Records.Count - MaxRecords)
            .Select(r => r.JobId)
            .ToList())
        {
            Records.Remove(key);
        }
    }
}

/// <summary>
/// Cooperative cancellation for long jobs. A page POSTs /jobs/{id}/cancel,
/// the worker checks <see cref="IsCancellationRequested"/> between units of
/// work (per video, per recipient, per lead) and stops cleanly.
/// </summary>
public static class JobCancellation
{
    private static readonly Dictionary<string, CancellationTokenSource> Sources = new(StringComparer.Ordinal);
    private static readonly object Gate = new();

    public static CancellationToken Register(string jobId)
    {
        lock (Gate)
        {
            var cts = new CancellationTokenSource();
            Sources[jobId] = cts;
            return cts.Token;
        }
    }

    public static void Cancel(string jobId)
    {
        lock (Gate)
        {
            if (Sources.TryGetValue(jobId, out var cts))
            {
                try { cts.Cancel(); } catch { }
            }
        }
        JobTrackerStore.Update(jobId, r =>
        {
            if (r.IsActive)
            {
                r.State = JobState.Cancelled;
                r.FinishedAt = DateTime.UtcNow;
                r.Message = "Cancelled. Partial progress above is kept.";
            }
        });
    }

    public static void Unregister(string jobId)
    {
        lock (Gate)
        {
            if (Sources.Remove(jobId, out var cts))
            {
                try { cts.Dispose(); } catch { }
            }
        }
    }

    public static bool IsCancellationRequested(string jobId)
    {
        lock (Gate)
        {
            return Sources.TryGetValue(jobId, out var cts) && cts.IsCancellationRequested;
        }
    }
}
