namespace TubeMailGorilla.Domain;

/// <summary>
/// Strongly-typed representation of the JWT configuration section.
/// </summary>
public class JwtSettings
{
    public string? Secret { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int ExpiryMinutes { get; set; } = 60;
}
