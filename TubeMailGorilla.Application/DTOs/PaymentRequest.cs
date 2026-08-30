namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Request payload for creating a payment order.
/// Amount/Currency are IGNORED for pricing (server-side configuration is
/// authoritative); UserId is populated by the controller from the JWT.
/// </summary>
public record PaymentRequest(
    decimal Amount = 0,
    string Currency = "USD",
    string? ReturnUrl = null,
    string? CancelUrl = null,
    string? UserId = null);
