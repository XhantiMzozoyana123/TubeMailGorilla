namespace TubeMailGorilla.Domain.Entities;

/// <summary>
/// Represents a user in the domain. This is a pure domain entity
/// with no dependency on any persistence or framework infrastructure.
/// </summary>
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
