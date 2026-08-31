using SQLite;

namespace TubeMailGorilla.Maui.Models;

public class Blocker
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string BlockedEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}