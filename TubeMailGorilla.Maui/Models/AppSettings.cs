namespace TubeMailGorilla.Maui.Models;

public class AppSettings
{
    public string? GeminiApiKey { get; set; }
    public int DefaultBatchSize { get; set; } = 50;
    public int DelayBetweenEmailsMs { get; set; } = 1000;
    public bool AutoOpenExternalLinks { get; set; } = true;
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
    public bool UseSystemTray { get; set; } = true;
}