using SQLite;

namespace TubeMailGorilla.Maui.Models;

public class Opener
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int EmailerId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}