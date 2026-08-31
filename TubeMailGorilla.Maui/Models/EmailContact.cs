using SQLite;

namespace TubeMailGorilla.Maui.Models;

public class EmailContact
{
    [PrimaryKey, AutoIncrement]
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
}