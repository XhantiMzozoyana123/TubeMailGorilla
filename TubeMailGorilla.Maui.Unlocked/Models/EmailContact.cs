using SQLite;

namespace TubeMailGorilla.Maui.Unlocked.Models;

/// <summary>
/// One snapshot as the contact editor shows it: the base64 JPEG plus the
/// transcript timestamp and words it was captured at. Kept separate from the
/// stored <see cref="EmailContact"/> shape so the view has exactly what it
/// binds to.
/// </summary>
public class VideoSnapshotItem
{
    public VideoSnapshotItem(string base64Image, string timestampLabel, double seconds)
    {
        Image = base64Image;
        TimestampLabel = timestampLabel;
        Seconds = seconds;
    }

    /// <summary>Base64 JPEG data (no data-URI prefix).</summary>
    public string Image { get; }

    /// <summary>"00:00:10" - when in the video this frame came from.</summary>
    public string TimestampLabel { get; }

    /// <summary>The same moment in seconds, used to seek the source video.</summary>
    public double Seconds { get; }
}

public class EmailContact
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Channel { get; set; }
    public string? VideoTitle { get; set; }
    public string? VideoDescription { get; set; }

    /// <summary>
    /// The YouTube URL the snapshots were taken from, so the editor can link
    /// each frame back to the moment it shows.
    /// </summary>
    public string? VideoUrl { get; set; }
    public DateTime ExtractedAt { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsEmailer { get; set; }
    public DateTime? LastEmailed { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Base64 JPEG snapshots of the lead's video, taken at the timestamps of
    /// its transcript cues. SQLite has no list type, so the list is persisted
    /// as a JSON array in this column and surfaced through
    /// <see cref="VideoSnapshot"/>.
    /// </summary>
    public string? VideoSnapshotJson { get; set; }

    /// <summary>
    /// The seek position (in seconds) each snapshot was taken at, parallel to
    /// <see cref="VideoSnapshotJson"/>. This is what the editor labels a frame
    /// with, e.g. "00:01:24", and seeks to for the "watch" link.
    /// </summary>
    public string? VideoSnapshotTimestampsJson { get; set; }

    /// <summary>
    /// The video snapshots, in transcript order. Exposed as a real
    /// <c>List&lt;string&gt;</c> for the edit view, which pages through them.
    /// </summary>
    [Ignore]
    public List<string> VideoSnapshot
    {
        get => DeserializeList(VideoSnapshotJson);
        set => VideoSnapshotJson = SerializeList(value);
    }

    /// <summary>
    /// The seek position in seconds for each snapshot, index-aligned with
    /// <see cref="VideoSnapshot"/>.
    /// </summary>
    [Ignore]
    public List<double> VideoSnapshotTimestamps
    {
        get => DeserializeTimestamps(VideoSnapshotTimestampsJson);
        set => VideoSnapshotTimestampsJson = value is null || value.Count == 0
            ? null
            : System.Text.Json.JsonSerializer.Serialize(value);
    }

    private static List<double> DeserializeTimestamps(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<double>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<double>>(json) ?? new List<double>();
        }
        catch
        {
            return new List<double>();
        }
    }

    /// <summary>True when at least one snapshot was captured for this lead.</summary>
    [Ignore]
    public bool HasVideoSnapshots => VideoSnapshot.Count > 0;

    private static List<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            // A corrupt/legacy value must never stop the contact from loading.
            return new List<string>();
        }
    }

    private static string? SerializeList(List<string>? values)
    {
        if (values is null || values.Count == 0) return null;
        return System.Text.Json.JsonSerializer.Serialize(values);
    }

    /// <summary>
    /// Never-empty label for the UI. Freshly scraped leads often have no name yet
    /// (37 of the first 60 did), so the contact list shows a friendly placeholder
    /// instead of a blank first line.
    /// </summary>
    [Ignore]
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Unnamed contact" : Name!;

    /// <summary>Avatar initials derived from the name, email or a "?" fallback.</summary>
    [Ignore]
    public string Initials
    {
        get
        {
            var source = !string.IsNullOrWhiteSpace(Name) ? Name!.Trim() : (Email ?? string.Empty).Trim();
            if (source.Length == 0) return "?";
            // Prefer first letters of first two words ("Some Channel" -> "SC").
            var parts = source.Split(new[] { ' ', '.', '_', '-', '@' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
        }
    }

    /// <summary>Never-empty channel line that also handles empty strings, not just null.</summary>
    [Ignore]
    public string DisplayChannel => string.IsNullOrWhiteSpace(Channel) ? "Channel: (unknown)" : $"Channel: {Channel}";

    /// <summary>One-line "channel • date" summary shown under the email address.</summary>
    [Ignore]
    public string DisplaySubtitle
    {
        get
        {
            var channel = string.IsNullOrWhiteSpace(Channel) ? "(unknown channel)" : Channel;
            return ExtractedAt == default ? channel : $"{channel} • {ExtractedAt:MMM d, yyyy}";
        }
    }
}