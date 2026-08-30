using TubeMailGorilla.Application.DTOs;

namespace TubeMailGorilla.Application.Interfaces;

/// <summary>
/// Application-layer use-case contract for payment and subscription operations.
/// </summary>
public interface ISubscriptionService
{
    Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);
    Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest request, string userId);

    /// <summary>
    /// Cancels the subscription by removing the active-subscription claim and
    /// issuing a fresh JWT that no longer carries it.
    /// </summary>
    Task<CancelSubscriptionResponse> CancelSubscriptionAsync(string userId);

        /// <summary>
    /// Returns whether the user holds the active-subscription claim. This is
    /// checked against the persisted Identity claim store (AspNetUserClaims),
    /// NOT the JWT, so it reflects a freshly-captured payment immediately
    /// without requiring re-login.
    /// </summary>
    Task<bool> IsSubscribedAsync(string userId);

    /// <summary>
    /// Returns the user's current subscription in customer-friendly terms
    /// (plan name, price, next billing date). Used by the website and the
    /// desktop app to display a single, consistent subscription state.
    /// </summary>
    Task<SubscriptionStatusResponse> GetStatusAsync(string userId);
}
