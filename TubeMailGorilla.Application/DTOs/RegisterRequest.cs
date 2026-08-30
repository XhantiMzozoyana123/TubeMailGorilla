namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Request payload for user registration.
/// Property names match the JSON contract consumed by the MAUI client.
/// </summary>
public record RegisterRequest(string Email, string Password, string? FullName);
