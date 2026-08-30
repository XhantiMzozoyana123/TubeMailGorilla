namespace TubeMailGorilla.Domain;

/// <summary>
/// A simple, framework-agnostic representation of a security claim.
/// Used to transfer claim data between the Domain layer and the
/// Infrastructure layer without leaking <see cref="System.Security.Claims.Claim"/>
/// into the domain model.
/// </summary>
public class ClaimInfo
{
    public string Type { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;

    public ClaimInfo() { }

    public ClaimInfo(string type, string value)
    {
        Type = type;
        Value = value;
    }
}
