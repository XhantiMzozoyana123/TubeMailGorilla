namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Response payload for payment order creation.
/// </summary>
public class PaymentResponse
{
    public bool Success { get; set; }
    public string? OrderId { get; set; }
    public string? ApprovalUrl { get; set; }
    public string? Message { get; set; }
}
