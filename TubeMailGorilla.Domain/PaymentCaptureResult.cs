namespace TubeMailGorilla.Domain;

/// <summary>
/// Verbatim result of a PayPal order capture. Carries what PayPal ACTUALLY
/// charged so the Application layer can verify it against the stored order
/// before granting any entitlement.
/// </summary>
public class PaymentCaptureResult
{
    public bool Success { get; set; }

    /// <summary>PayPal status of the order after capture (e.g. COMPLETED).</summary>
    public string? Status { get; set; }

    /// <summary>PayPal capture resource id.</summary>
    public string? CaptureId { get; set; }

    public decimal CapturedAmount { get; set; }

    public string? Currency { get; set; }
}