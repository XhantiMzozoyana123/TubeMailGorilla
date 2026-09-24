using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// The campaign composer - the web twin of the MAUI Unlocked <c>SendEmailsPage</c>:
/// template picker, token chips, sending options, message rotation and the send loop.
/// Unlocked edition: anonymous and unlimited.
/// </summary>
[AllowAnonymous]
public class SendEmailsController : Controller
{
    private readonly DatabaseService _db;
    private readonly EmailService _email;
    private readonly ValidationService _validation;

    public SendEmailsController(DatabaseService db, EmailService email, ValidationService validation)
    {
        _db = db;
        _email = email;
        _validation = validation;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await BuildModelAsync());

    /// <summary>Auto-posted when a rotation switch or the account picker changes.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveOptions(bool allowAccountRotation, int defaultSenderId, bool allowMessageRotation)
    {
        SendSettings.AllowAccountRotation = allowAccountRotation;
        SendSettings.AllowMessageRotation = allowMessageRotation;
        if (!allowAccountRotation)
            SendSettings.DefaultSenderId = defaultSenderId; // 0 = "(first active account)"
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Adds a message variation (desktop OnSaveVariationClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveVariation(string varSubject, string varBody)
    {
        var subject = varSubject?.Trim() ?? string.Empty;
        var body = varBody?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
        {
            var model = await BuildModelAsync();
            model.VarSubject = subject;
            model.VarBody = body;
            model.ShowVariationEditor = true;
            model.Error = "Give the variation both a subject and a message.";
            return View(nameof(Index), model);
        }

        var variations = SendSettings.MessageVariations;
        variations.Add(new MessageVariation(subject, body));
        SendSettings.MessageVariations = variations;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Removes one variation (desktop OnRemoveVariationClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveVariation(int index)
    {
        var variations = SendSettings.MessageVariations;
        if (index >= 0 && index < variations.Count)
        {
            variations.RemoveAt(index);
            SendSettings.MessageVariations = variations;
        }
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Runs the campaign - ported from SendEmailsPage.OnSendEmailsClicked.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string? subject, string? body)
    {
        subject = subject?.Trim() ?? string.Empty;
        body = body?.Trim() ?? string.Empty;

        var usingRotation = SendSettings.AllowMessageRotation && SendSettings.MessageVariations.Any(v =>
            !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body));

        if (!usingRotation && (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body)))
        {
            var model = await BuildModelAsync();
            model.Subject = subject;
            model.Body = body;
            model.Error = "Please write both a subject and an email body first (or pick a template above).";
            return View(nameof(Index), model);
        }

        // Save the compose draft as the new default (desktop Preferences EmailSubject/EmailBody).
        if (!string.IsNullOrWhiteSpace(subject)) WebPreferences.Set("EmailSubject", subject);
        if (!string.IsNullOrWhiteSpace(body)) WebPreferences.Set("EmailBody", body);

        try
        {
            var allContacts = await _db.GetContactsAsync();
            var senders = await _db.GetSendersAsync();

            // GATEKEEPER: approved BEFORE any email is sent.
            var verdict = await _validation.CheckAsync(ValidationService.SendEmails, allContacts.Count);
            var cappedByPlan = verdict.Limit >= 0 && allContacts.Count > verdict.Limit;
            var contacts = cappedByPlan ? allContacts.Take(verdict.Limit).ToList() : allContacts;

            if (senders.Count == 0)
            {
                var noAccounts = await BuildModelAsync();
                noAccounts.Subject = subject;
                noAccounts.Body = body;
                noAccounts.Error = "No active email accounts. Add one in Settings \u2192 Email Accounts.";
                return View(nameof(Index), noAccounts);
            }

            if (contacts.Count == 0)
            {
                var noContacts = await BuildModelAsync();
                noContacts.Subject = subject;
                noContacts.Body = body;
                noContacts.Error = "No contacts to email. Extract leads from the Extract page first.";
                return View(nameof(Index), noContacts);
            }

            var parameters = await _db.GetMessageParametersAsync();
            var sent = 0;
            var failed = 0;
            var skipped = 0;

            // Message rotation: each variation takes its turn across recipients.
            var variations = SendSettings.MessageVariations.Where(v =>
                !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body)).ToList();
            var rotateMessages = usingRotation && variations.Count > 0;

            // Each lead's latest AI icebreaker so [icebreaker] resolves per recipient.
            var openers = await _db.GetOpenersAsync();
            var latestOpenerByContact = openers
                .Where(o => o.EmailerId > 0 && !string.IsNullOrWhiteSpace(o.Text))
                .GroupBy(o => o.EmailerId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(o => o.CreatedAt).First().Text.Trim());

            for (int i = 0; i < contacts.Count; i++)
            {
                var contact = contacts[i];

                // Leads without an icebreaker are skipped, exactly like the desktop loop.
                if (!latestOpenerByContact.TryGetValue(contact.Id, out var icebreaker))
                {
                    skipped++;
                    continue;
                }

                // Rotate accounts when enabled; otherwise the configured default (or first).
                Sender senderAccount;
                if (SendSettings.AllowAccountRotation && senders.Count > 1)
                {
                    senderAccount = senders[i % senders.Count];
                }
                else
                {
                    senderAccount = senders[0];
                    var defaultId = SendSettings.DefaultSenderId;
                    if (defaultId > 0)
                        senderAccount = senders.FirstOrDefault(s => s.Id == defaultId) ?? senders[0];
                }

                string messageSubject = subject;
                string messageBody = body;
                if (rotateMessages)
                {
                    var variation = variations[i % variations.Count];
                    messageSubject = variation.Subject;
                    messageBody = variation.Body;
                }

                var message = new MessengerDto
                {
                    EmailFrom = senderAccount.EmailAddress,
                    FromName = senderAccount.Name,
                    EmailTo = contact.Email,
                    ToName = contact.Name ?? contact.Email,
                    Subject = EmailService.Personalize(messageSubject, contact, parameters, icebreaker),
                    Body = EmailService.Personalize(messageBody, contact, parameters, icebreaker),
                    SmtpHost = senderAccount.SmtpHost,
                    SmtpPort = senderAccount.SmtpPort,
                    SmtpUser = senderAccount.SmtpUser,
                    SmtpPassword = senderAccount.SmtpPassword
                };

                if (await _email.SendEmailAsync(message)) sent++;
                else failed++;

                // Small delay to avoid hitting rate limits.
                await Task.Delay(500);
            }

            TempData["StatusMessage"] = cappedByPlan
                ? $"All done. Sent: {sent}, Failed: {failed}, Skipped (no icebreaker/blocked): {skipped}. " +
                  $"Your plan targets the first {verdict.Limit} of {allContacts.Count} leads."
                : $"All done. Sent: {sent}, Failed: {failed}, Skipped (no icebreaker/blocked): {skipped}";
        }
        catch (Exception ex)
        {
            TempData["StatusError"] = $"Send failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------ helpers


    private async Task<SendEmailsViewModel> BuildModelAsync()
    {
        var contacts = await _db.GetContactsAsync();
        var accounts = await _db.GetAllSendersAsync();
        var templates = await _db.GetTemplatesAsync();

        var model = new SendEmailsViewModel
        {
            RecipientCount = contacts.Count,
            Subject = WebPreferences.Get("EmailSubject", string.Empty),
            Body = WebPreferences.Get("EmailBody", string.Empty),
            Templates = templates.OrderBy(t => t.Name).ToList(),
            AllowAccountRotation = SendSettings.AllowAccountRotation,
            AllowMessageRotation = SendSettings.AllowMessageRotation,
            DefaultSenderId = SendSettings.DefaultSenderId,
            ActiveAccounts = accounts.Where(a => a.IsActive).ToList(),
            Variations = SendSettings.MessageVariations
        };

        model.TemplateStatus = model.Templates.Count == 0
            ? "You have no saved templates yet. Create them on the Email Templates page."
            : $"{model.Templates.Count} template{(model.Templates.Count == 1 ? "" : "s")} available.";

        return model;
    }
}
