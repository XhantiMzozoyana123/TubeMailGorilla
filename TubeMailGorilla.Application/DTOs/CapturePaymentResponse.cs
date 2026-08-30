namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Response payload returned after capturing a payment.
/// </summary>
public class CapturePaymentResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// A freshly-issued JWT that already contains the subscription claim.
    /// The client should replace its stored token with this one so protected
    /// endpoints ([Authorize(Policy = "Subscribed")]) work immediately,
    /// without logging out and back in.
    /// </summary>
    public string? Token { get; set; }
}

/// <summary>
/// Response payload returned after cancelling a subscription.
/// </summary>
public class CancelSubscriptionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    /// <summary>
    /// True if the user no longer holds the active-subscription claim.
    /// </summary>
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// A freshly-issued JWT WITHOUT the subscription claim. The client should
    /// replace its stored token so premium endpoints reject immediately.
    /// </summary>
    public string? Token { get; set; }
}
