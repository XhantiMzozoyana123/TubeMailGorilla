namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Response payload returned for both register and login operations.
/// Property names are camelCase-friendly to match the MAUI client deserializer.
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
}
