namespace TubeMailGorilla.Maui.Unlocked.Models;

/// <summary>Tappable token chip shown above the subject / body editors.</summary>
public sealed record TokenOption(string Label, string Token);

/// <summary>Which compose field last had focus (for token insertion).</summary>
public enum FocusedField
{
    Subject,
    Body
}
