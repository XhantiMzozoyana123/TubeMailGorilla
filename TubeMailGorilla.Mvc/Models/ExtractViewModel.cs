using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Models;

/// <summary>View model for the Extract page - mirrors the desktop ExtractPage state.</summary>
public class ExtractViewModel
{
    public string? Keyword { get; set; }
    public int PageViewLimit { get; set; } = 5;
    public bool GmailOnly { get; set; }
    public bool ValidateEmails { get; set; } = true;

    public string? StatusMessage { get; set; }
    public string? Error { get; set; }
    public string? LlmStatus { get; set; }

    /// <summary>Status line for the bulk CSV run (BulkStatusLabel on desktop).</summary>
    public string? BulkStatusMessage { get; set; }

    public int TotalVideos { get; set; }
    public int EmailsFound { get; set; }
    public int Errors { get; set; }
    public TimeSpan Elapsed { get; set; }

    public bool YtDlpAvailable { get; set; }
    public EntitlementInfo? Entitlements { get; set; }
    public List<EmailContact> Contacts { get; set; } = new();

    /// <summary>RESULTS card line - same format as the desktop ResultsLabel.</summary>
    public string? ResultsLabel =>
        TotalVideos > 0 || EmailsFound > 0
            ? $"Videos: {TotalVideos}  |  Emails: {EmailsFound}  |  Errors: {Errors}"
            : null;
}

