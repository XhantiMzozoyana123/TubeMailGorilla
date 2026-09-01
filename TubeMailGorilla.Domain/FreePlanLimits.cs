namespace TubeMailGorilla.Domain;

/// <summary>
/// Limits applied to FREE (non-paying) users. Configured via appsettings.json
/// "FreePlan" and exposed to clients through GET /api/payments/entitlements so
/// the website and desktop app always agree with the server.
/// </summary>
public class FreePlanLimits
{
    // NOTE: There is deliberately NO per-extraction lead cap on the free plan.
    // Free users may pull as many leads as a single run finds, but the server
    // throttles them to exactly ONE extraction per calendar month (see
    // ExtractionUsageService / ValidationController).

    /// <summary>Maximum contacts visible on the contacts page.</summary>
    public int MaxContactsVisible { get; set; } = 5;

    /// <summary>Maximum recipients per send campaign.</summary>
    public int MaxEmailsPerCampaign { get; set; } = 5;
}