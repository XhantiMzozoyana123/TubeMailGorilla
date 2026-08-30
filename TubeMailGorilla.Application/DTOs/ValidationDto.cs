namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Actions that require the API's approval before the desktop app performs
/// any workload. Keep this list tight - unknown actions are always denied.
/// </summary>
public static class ValidationAction
{
    public const string ExtractLeads = "extract_leads";
    public const string SendEmails = "send_emails";
    public const string ViewContacts = "view_contacts";
    public const string GenerateIcebreaker = "generate_icebreaker";
    public const string UseEmailTemplates = "use_email_templates";
    public const string UseBlocklist = "use_blocklist";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        ExtractLeads, SendEmails, ViewContacts, GenerateIcebreaker, UseEmailTemplates, UseBlocklist
    };
}

/// <summary>Client asks permission before doing work.</summary>
public record ValidationRequest(string Action, int RequestedAmount = 0);

/// <summary>
/// The gatekeeper's verdict. Approved = green light to continue with the
/// workload; Denied = the Reason explains why (usually "please upgrade").
/// </summary>
public record ValidationResponse(
    bool Approved,
    string Action,
    string Reason,
    string PlanId,
    string PlanName,
    int Limit,
    int RequestedAmount);