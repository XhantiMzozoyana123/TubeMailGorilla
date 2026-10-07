namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Minimal copy of the app's TranscriptCue - only what the remux test needs.
/// (The real one lives at the end of YouTubeTranscriptService.cs.)
/// </summary>
public class TranscriptCue
{
    public TranscriptCue(double startSeconds, double endSeconds, string text)
    {
        StartSeconds = startSeconds;
        EndSeconds = endSeconds;
        Text = text;
    }

    public double StartSeconds { get; }
    public double EndSeconds { get; }
    public string Text { get; }

    public string TimestampLabel => FormatTimestamp(StartSeconds);

    public static string FormatTimestamp(double seconds)
    {
        var total = (int)Math.Max(0, seconds);
        return $"{total / 3600:00}:{total / 60 % 60:00}:{total % 60:00}";
    }
}
