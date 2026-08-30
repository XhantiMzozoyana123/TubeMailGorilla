namespace TubeMailGorilla.Domain;

/// <summary>
/// Limits applied to FREE (non-paying) users. Configured via appsettings.json
/// "FreePlan" and exposed to clients through GET /api/payments/entitlements so
/// the website and desktop app always agree with the server.
/// </summary>
public class FreePlanLimits
{
    /// <summary>Maximum leads a single extraction run may produce.</summary>
    public int MaxLeadsPerExtraction { get; set; } = 5;

    /// <summary>Maximum contacts visible on the contacts page.</summary>
    public int MaxContactsVisible { get; set; } = 5;

    /// <summary>Maximum recipients per send campaign.</summary>
    public int MaxEmailsPerCampaign { get; set; } = 5;
}