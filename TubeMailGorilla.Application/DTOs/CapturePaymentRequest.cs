namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Request payload for capturing an approved PayPal order.
/// </summary>
public record CapturePaymentRequest(string OrderId);
