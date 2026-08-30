namespace TubeMailGorilla.Domain;

/// <summary>
/// Base class for domain entities providing common audit fields.
/// </summary>
public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DateModified { get; set; }
}
