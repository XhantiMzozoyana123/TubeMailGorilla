namespace TubeMailGorilla.Mvc.Models;

/// <summary>
/// One alternate subject/body pair for message rotation. When
/// <c>AllowMessageRotation</c> is on and this list is non-empty, the
/// campaign worker picks variation <c>i % count</c> per recipient instead
/// of the base <c>Subject</c>/<c>Body</c>.
/// </summary>
public sealed class MessageVariation
{
    /// <summary>Subject line for the rotated message.</summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>Body text for the rotated message.</summary>
    public string Body { get; set; } = string.Empty;
}
