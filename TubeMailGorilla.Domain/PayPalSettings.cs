namespace TubeMailGorilla.Domain;

/// <summary>
/// Strongly-typed representation of PayPal configuration.
/// </summary>
public class PayPalSettings
{
    public string? ClientId { get; set; }
    public string? Secret { get; set; }
    public string Mode { get; set; } = "sandbox"; // "sandbox" or "live"

    /// <summary>
    /// Id of the PayPal billing plan (P-XXXX…) to subscribe users to.
    /// Created once in the PayPal dashboard/API and pinned here - all
    /// recurring checkouts reference this exact plan.
    /// </summary>
    public string? PlanId { get; set; }
}

/// <summary>
/// Server-side subscription pricing. The client NEVER dictates the amount;
/// <see cref="SubscriptionService"/> reads this configuration when creating
/// PayPal orders so prices cannot be tampered with from the caller side.
/// </summary>
public class PricingSettings
{
    public decimal Amount { get; set; } = 9.99m;
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// A single user-facing subscription plan shown on the website and in the
/// desktop app. Defined once in appsettings.json ("SubscriptionPlans") so
/// marketing/pricing changes never require code changes.
/// </summary>
public class SubscriptionPlanDefinition
{
    /// <summary>Stable machine id, e.g. "pro".</summary>
    public string Id { get; set; } = "pro";

    /// <summary>User-facing display name, e.g. "Pro".</summary>
    public string Name { get; set; } = "Pro";

    /// <summary>Short one-line pitch shown under the plan name.</summary>
    public string Tagline { get; set; } = string.Empty;

    /// <summary>Monthly recurring price charged via PayPal.</summary>
    public decimal MonthlyPrice { get; set; } = 9.99m;

    public string Currency { get; set; } = "USD";

    /// <summary>Creator leads included per billing month.</summary>
    public int LeadsPerMonth { get; set; } = 5000;

    /// <summary>Benefit bullets shown to the user (plain language, no tech).</summary>
    public string[] Features { get; set; } = System.Array.Empty<string>();

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// The full catalog of plans configured in appsettings.json.
/// </summary>
public class SubscriptionPlansOptions
{
    public List<SubscriptionPlanDefinition> Plans { get; set; } = new();

    /// <summary>The plan new subscribers are signed up to (first enabled one).</summary>
    public SubscriptionPlanDefinition? DefaultPlan =>
        Plans.FirstOrDefault(p => p.IsEnabled);
}
