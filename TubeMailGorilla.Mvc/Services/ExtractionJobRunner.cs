using Hangfire;
using Hangfire.Storage;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Services;

/// <summary>
/// Runs lead extraction on a Hangfire background worker instead of inside the
/// HTTP request.
///
/// Why: one extraction is not a request-sized operation. Per video the pipeline
/// fetches the description, pulls a transcript (yt-dlp, 120s cap), downloads
/// the video for snapshots (4 min cap), decodes frames (3 min cap) and then
/// makes several Ollama calls that each get up to 300s. Run sequentially over
/// five videos that is routinely tens of minutes - far past any reverse-proxy
/// or Kestrel timeout, so the browser saw a timeout even though the server
/// kept working.
///
/// The job writes its progress to a small status record so the Extract page can
/// poll and show where a run has got to, and survive a page refresh.
/// </summary>
public class ExtractionJobRunner
{
    private readonly ExtractService _extract;
    private readonly ILogger<ExtractionJobRunner> _log;

    public ExtractionJobRunner(ExtractService extract, ILogger<ExtractionJobRunner> log)
    {
        _extract = extract;
        _log = log;
    }

    /// <summary>
    /// Hangfire entry point. Never throws - failures are recorded.
    ///
    /// Progress is tracked against the CURRENT run rather than against Hangfire's
    /// job id. Hangfire only substitutes a real id into a parameter it explicitly
    /// recognises (PerformContext / IJobContext / [JobId]); any other argument is
    /// passed through verbatim, so an earlier "string jobId" parameter silently
    /// arrived as null and every status lookup threw ArgumentNullException. Since
    /// extraction is serialised (one worker, one queue) there is only ever one
    /// current run, so a single slot is both simpler and version-independent.
    /// </summary>
    public async Task RunAsync(string keyword, int pageViewLimit,
        bool gmailOnly, bool validateEmails)
    {
        var started = DateTime.UtcNow;

        Update(j =>
        {
            j.State = "Running";
            j.Keyword = keyword;
            j.StartedAt = started;
            j.Message = "Searching YouTube...";
        });

        try
        {
            var result = await _extract.ExtractByKeywordAsync(
                keyword, pageViewLimit, gmailOnly, validateEmails);

            Update(j =>
            {
                j.State = "Completed";
                j.FinishedAt = DateTime.UtcNow;
                j.TotalVideos = result.TotalVideos;
                j.EmailsFound = result.EmailsFound;
                j.Errors = result.Errors;
                j.Message = $"Done. Videos: {result.TotalVideos}, leads: {result.EmailsFound}, errors: {result.Errors}.";
            });
        }
        catch (Exception ex)
        {
            // Hangfire retries a throwing job, and a permanently bad keyword would
            // retry forever. The status record carries the reason instead.
            _log.LogError(ex, "Extraction job for '{Keyword}' failed.", keyword);

            Update(j =>
            {
                j.State = "Failed";
                j.FinishedAt = DateTime.UtcNow;
                j.Message = "Extraction failed: " + ex.Message;
            });
        }
    }

    private void Update(Action<ExtractionRunStatus> mutate)
    {
        var status = ExtractionRunStore.Current ?? new ExtractionRunStatus();
        mutate(status);
        ExtractionRunStore.Save(status);
    }
}

/// <summary>One extraction run's progress, for the Extract page to poll.</summary>
public class ExtractionRunStatus
{
    public string JobId { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;

    /// <summary>Queued / Running / Completed / Failed.</summary>
    public string State { get; set; } = "Queued";

    public string Message { get; set; } = string.Empty;
    public int TotalVideos { get; set; }
    public int EmailsFound { get; set; }
    public int Errors { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string ElapsedLabel
    {
        get
        {
            var end = FinishedAt ?? DateTime.UtcNow;
            if (StartedAt is null) return "";

            var span = end - StartedAt.Value;
            return span.TotalHours >= 1
                ? $"{(int)span.TotalHours}h {span.Minutes}m"
                : $"{(int)span.TotalMinutes}m {span.Seconds}s";
        }
    }

    public bool IsActive => State is "Queued" or "Running";
}

/// <summary>
/// Process-wide store of recent extraction runs.
///
/// Deliberately in memory rather than in the Hangfire storage: the dashboard
/// already holds the durable job history, and a restart only costs the UI a
/// running-progress line for jobs that are themselves recovered by Hangfire.
/// </summary>
public static class ExtractionRunStore
{
    private const int MaxRuns = 25;
    private static readonly Dictionary<string, ExtractionRunStatus> Runs = new(StringComparer.Ordinal);
    private static readonly object Gate = new();
    private static ExtractionRunStatus? _current;

    public static void Save(ExtractionRunStatus status)
    {
        lock (Gate)
        {
            _current = status;
            if (!string.IsNullOrEmpty(status.JobId))
                Runs[status.JobId] = status;

            // Bound it: the extract page only ever shows the most recent runs.
            if (Runs.Count > MaxRuns)
            {
                var oldest = Runs.OrderBy(kv => kv.Value.StartedAt ?? DateTime.MinValue)
                                 .First().Key;
                Runs.Remove(oldest);
            }
        }
    }

    /// <summary>The run currently executing or most recently finished.</summary>
    public static ExtractionRunStatus? Current
    {
        get
        {
            lock (Gate)
            {
                return _current;
            }
        }
    }
    /// <summary>
    /// Looks a run up by id. Tolerates null/empty so a caller that has lost the id
    /// falls back to <see cref="Current"/> instead of throwing.
    /// </summary>
    public static ExtractionRunStatus? Get(string? jobId)
    {
        lock (Gate)
        {
            if (string.IsNullOrEmpty(jobId)) return _current;
            return Runs.TryGetValue(jobId, out var run) ? run : _current;
        }
    }

    public static List<ExtractionRunStatus> Recent(int take = 10)
    {
        lock (Gate)
        {
            return Runs.Values
                .OrderByDescending(r => r.StartedAt ?? DateTime.MinValue)
                .Take(take)
                .ToList();
        }
    }
}

