using SQLite;

namespace TubeMailGorilla.Maui.Models;

public class Inboxer
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int EmailerId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsRead { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.Now;
    public DateTime? RepliedAt { get; set; }
}