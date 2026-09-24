using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Models;

/// <summary>View model for the Settings page (SettingsPage).</summary>
public class SettingsViewModel
{
    // EXTRACTION card -------------------------------------------------------
    public bool GmailOnly { get; set; }
    public bool ValidateEmails { get; set; } = true;

    /// <summary>Default page limit offered on the Extract page.</summary>
    public int DefaultPageLimit { get; set; } = 5;

    // EMAIL SHORTCODES card -------------------------------------------------
    public List<MessageParameter> Parameters { get; set; } = new();
    public int SelectedParameterId { get; set; }
    public string CustomToken { get; set; } = string.Empty;

    /// <summary>Friendly picker options -&gt; internal data field (same list as MAUI).</summary>
    public static readonly (string Label, string Field)[] AddableFields =
    {
        ("Their first name", "first-name"),
        ("Their last name", "last-name"),
        ("Their full name", "name"),
        ("Their email address", "email"),
        ("Their YouTube channel name", "channel-name"),
        ("Their latest video title", "video-title"),
        ("Their video description", "video-description"),
        ("An AI icebreaker first line", "icebreaker"),
    };

    /// <summary>Plain-English description for a field (FieldToDescriptionConverter).</summary>
    public static string DescribeField(string field)
    {
        foreach (var (label, f) in AddableFields)
            if (f.Equals(field, StringComparison.OrdinalIgnoreCase))
                return label switch
                {
                    "Their first name" => "The lead's first name",
                    "Their last name" => "The lead's last name",
                    "Their full name" => "The lead's full name",
                    "Their email address" => "The lead's email address",
                    "Their YouTube channel name" => "Their YouTube channel name",
                    "Their latest video title" => "Their latest video title",
                    "Their video description" => "Their video description",
                    _ => "AI-written personalized first line"
                };
        return $"Lead data: {field}";
    }

    // EMAIL ACCOUNTS card ---------------------------------------------------
    public List<Sender> Senders { get; set; } = new();

    // EDITION card ----------------------------------------------------------
    public string EditionLabel { get; set; } = string.Empty;
    public string EditionDescription { get; set; } = string.Empty;
}

/// <summary>View model for the Blocked page (BlockedPage).</summary>
public class BlockedViewModel
{
    public List<Blocker> Blockers { get; set; } = new();
    public string EmailInput { get; set; } = string.Empty;
    public string? Error { get; set; }
}

/// <summary>View model for the email template details page (EmailTemplateDetailsPage).</summary>
public class TemplateDetailsViewModel
{
    public EmailTemplate Template { get; set; } = new();
    public bool IsNew => Template.Id == 0;
    public string? Error { get; set; }
}
