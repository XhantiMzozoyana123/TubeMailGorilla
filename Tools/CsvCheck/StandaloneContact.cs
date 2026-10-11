namespace TubeMailGorilla.Maui.Unlocked.Models;

// Minimal stand-in for the app's EmailContact: only the fields the CSV
// importer/exporter touches. The harness compiles the REAL
// ContactCsvService.cs against this shape, so the round-trip behaviour
// tested here is the behaviour that ships.
public class EmailContact
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Channel { get; set; }
    public string? VideoTitle { get; set; }
    public string? VideoDescription { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime ExtractedAt { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? LastEmailed { get; set; }
    public DateTime UpdatedAt { get; set; }
}
