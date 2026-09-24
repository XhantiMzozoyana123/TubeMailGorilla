
namespace TubeMailGorilla.Mvc.Models;

public class EmailContact
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Channel { get; set; }
    public string? VideoTitle { get; set; }
    public string? VideoDescription { get; set; }
    public DateTime ExtractedAt { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsEmailer { get; set; }
    public DateTime? LastEmailed { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Never-empty label for the UI. Freshly scraped leads often have no name yet,
    /// so the contact list shows a friendly placeholder instead of a blank first line.
    /// (Mirrors the MAUI model's [Ignore] DisplayName.)
    /// </summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Unnamed contact" : Name!;

    /// <summary>Avatar initials derived from the name, email or a "?" fallback.</summary>
    public string Initials
    {
        get
        {
            var source = !string.IsNullOrWhiteSpace(Name) ? Name!.Trim() : Email.Trim();
            if (source.Length == 0) return "?";
            // Prefer first letters of first two words ("Some Channel" -> "SC").
            var parts = source.Split(new[] { ' ', '.', '_', '-', '@' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        }
    }

    /// <summary>Never-empty channel line that also handles empty strings, not just null.</summary>
    public string DisplayChannel => string.IsNullOrWhiteSpace(Channel) ? "Channel: (unknown)" : $"Channel: {Channel}";

    /// <summary>One-line "channel • date" summary shown under the email address.</summary>
    public string DisplaySubtitle
    {
        get
        {
            var channel = string.IsNullOrWhiteSpace(Channel) ? "(unknown channel)" : Channel;
            return ExtractedAt == default ? channel : $"{channel} • {ExtractedAt:MMM d, yyyy}";
        }
    }
}
