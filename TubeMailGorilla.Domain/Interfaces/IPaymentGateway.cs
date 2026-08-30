using TubeMailGorilla.Domain;

namespace TubeMailGorilla.Domain.Interfaces;

/// <summary>Result of starting a recurring PayPal subscription checkout.</summary>
public class SubscriptionCheckoutResult
{
    public bool Success { get; set; }

    /// <summary>PayPal subscription id (the durable reference to cancel later).</summary>
    public string? PayPalSubscriptionId { get; set; }

    /// <summary>URL the buyer must visit to approve the recurring payment.</summary>
    public string? ApprovalUrl { get; set; }

    public string? Error { get; set; }
}

/// <summary>Verbatim remote state of a PayPal subscription.</summary>
public class RemoteSubscriptionState
{
    public bool Success { get; set; }

    /// <summary>APPROVED | ACTIVE | SUSPENDED | CANCELLED | EXPIRED …</summary>
    public string? Status { get; set; }

    public decimal LastPaymentAmount { get; set; }

    public string? Currency { get; set; }

    public string? Error { get; set; }
}

/// <summary>
/// Defines the contract for a RECURRING payment gateway integration (e.g.
/// PayPal Subscriptions API). Implementations live in the Infrastructure
/// layer, keeping the Domain layer free of payment-provider SDK dependencies.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Ensures the billing product + monthly plan exist at PayPal (idempotent),
    /// then creates a subscription for the buyer to approve. Returns the
    /// PayPal subscription id and approval URL.
    /// </summary>
    Task<SubscriptionCheckoutResult> StartSubscriptionAsync(string returnUrl, string cancelUrl);

    /// <summary>
    /// Fetches the verbatim remote status of a PayPal subscription.
    /// Used to verify approval before granting entitlements.
    /// </summary>
    Task<RemoteSubscriptionState> GetRemoteStatusAsync(string payPalSubscriptionId);

    /// <summary>
    /// Cancels the subscription AT PAYPAL so no further recurring charges occur.
    /// Returns true only when PayPal confirmed the cancellation.
    /// </summary>
    Task<bool> CancelRemoteSubscriptionAsync(string payPalSubscriptionId, string reason);
}
