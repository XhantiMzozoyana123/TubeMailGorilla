using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Models;

/// <summary>View model for the Send Emails page - mirrors SendEmailsPage state.</summary>
public class SendEmailsViewModel
{
    // Header ---------------------------------------------------------------
    public int RecipientCount { get; set; }
    public string RecipientsLabel => $"{RecipientCount} {(RecipientCount == 1 ? "lead" : "leads")}";
    public bool HasContacts => RecipientCount > 0;

    // 1 - MESSAGE ----------------------------------------------------------
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int SelectedTemplateId { get; set; }
    public List<EmailTemplate> Templates { get; set; } = new();
    public string TemplateStatus { get; set; } = string.Empty;

    // 2 - SENDING ----------------------------------------------------------
    public bool AllowAccountRotation { get; set; }
    public int DefaultSenderId { get; set; }
    public List<Sender> ActiveAccounts { get; set; } = new();

    // 3 - MESSAGE ROTATION -------------------------------------------------
    public bool AllowMessageRotation { get; set; }
    public List<MessageVariation> Variations { get; set; } = new();
    public string VarSubject { get; set; } = string.Empty;
    public string VarBody { get; set; } = string.Empty;
    public bool ShowVariationEditor { get; set; }

    // 4 - REVIEW AND SEND --------------------------------------------------
    public string SendSummary { get; set; } = "Review your message, then send.";
    public string? StatusMessage { get; set; }
    public string? Error { get; set; }

    /// <summary>True when rotation is on and at least one valid variation exists -</summary>
    public bool UsingRotation => AllowMessageRotation && Variations.Any(v =>
        !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body));
}
