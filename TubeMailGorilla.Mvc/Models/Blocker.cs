
namespace TubeMailGorilla.Mvc.Models;

public class Blocker
{
    public int Id { get; set; }
    public string BlockedEmail { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}