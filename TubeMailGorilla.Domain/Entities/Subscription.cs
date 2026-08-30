namespace TubeMailGorilla.Domain.Entities;

/// <summary>
/// A subscription lifecycle record persisted per user.
/// Tracks each PayPal order from creation through activation/cancellation so
/// captures can be verified against what was originally ordered.
/// </summary>
public class Subscription
{
    public int Id { get; set; }

    /// <summary>ASP.NET Identity user id.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>PayPal checkout order id (token in the approval URL).</summary>
    public string PayPalOrderId { get; set; } = string.Empty;

    /// <summary>PayPal capture id once payment completed (null until then).</summary>
    public string? PayPalCaptureId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    /// <summary>Pending | Active | Cancelled</summary>
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ActivatedAtUtc { get; set; }

    public DateTime? CancelledAtUtc { get; set; }
}