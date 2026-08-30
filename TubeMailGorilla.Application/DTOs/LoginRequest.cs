namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Request payload for user login.
/// </summary>
public record LoginRequest(string Email, string Password);
