namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// The user's current subscription in plain, customer-friendly terms.
/// Served by GET /api/payments/status so the website and the desktop app
/// both display exactly the same subscription state.
/// </summary>
public class SubscriptionStatusResponse
{
    public bool IsSubscribed { get; set; }

    /// <summary>Plan identifier, e.g. "pro" — or "free" when not subscribed.</summary>
    public string PlanId { get; set; } = "free";

    /// <summary>User-facing plan name, e.g. "Pro".</summary>
    public string PlanName { get; set; } = "Free";

    public string? Tagline { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    /// <summary>Estimated next charge date (activation + 1 billing month).</summary>
    public DateTime? NextBillingDate { get; set; }
}
