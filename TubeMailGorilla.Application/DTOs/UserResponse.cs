namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// Response payload for the current-user endpoint.
/// </summary>
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public bool Success { get; set; }
}
