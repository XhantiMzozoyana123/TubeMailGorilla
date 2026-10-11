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
    private readonly BackgroundJobService _jobs;

    public SendEmailsController(
        DatabaseService db, EmailService email, ValidationService validation,
        BackgroundJobService jobs)
    {
        _db = db;
        _email = email;
        _validation = validation;
        _jobs = jobs;
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
        variations.Add(new MessageVariation { Subject = subject, Body = body });
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

    /// <summary>
    /// Queues the campaign as a background job and redirects to Jobs.
    /// The send loop (SMTP + per-recipient vision calls) takes minutes, so it
    /// must never run inside the request.
    /// </summary>
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

        // Single-flight: two campaigns would double-send to the same leads.
        if (JobTrackerStore.HasActive(JobKind.Campaign))
        {
            var busy = await BuildModelAsync();
            busy.Subject = subject;
            busy.Body = body;
            busy.Error = "A campaign is already sending. Track it on the Jobs page.";
            return View(nameof(Index), busy);
        }

        try
        {
            var allContacts = await _db.GetContactsAsync();
            var senders = await _db.GetSendersAsync();

            // GATEKEEPER: approved BEFORE anything is queued.
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

            var request = new CampaignRequest
            {
                Subject = subject,
                Body = body,
                ContactIds = contacts.Select(c => c.Id).ToList(),
                AllowAccountRotation = SendSettings.AllowAccountRotation,
                DefaultSenderId = SendSettings.DefaultSenderId,
                AllowMessageRotation = SendSettings.AllowMessageRotation,
                Variations = SendSettings.MessageVariations
                    .Where(v => !string.IsNullOrWhiteSpace(v.Subject) && !string.IsNullOrWhiteSpace(v.Body))
                    .ToList()
            };

            _jobs.EnqueueCampaign(request);
            TempData["StatusMessage"] =
                $"Campaign queued for {contacts.Count} leads. You can close the browser - progress is on the Jobs page.";
            return RedirectToAction("Index", "Jobs");
        }
        catch (Exception ex)
        {
            TempData["StatusError"] = $"Could not queue the campaign: {ex.Message}";
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
            Variations = SendSettings.MessageVariations.ToList(),
            ActiveCampaign = JobTrackerStore.LatestActive(JobKind.Campaign),
            RecentCampaigns = JobTrackerStore.Recent(JobKind.Campaign, take: 5)
        };

        model.TemplateStatus = model.Templates.Count == 0
            ? "You have no saved templates yet. Create them on the Email Templates page."
            : $"{model.Templates.Count} template{(model.Templates.Count == 1 ? "" : "s")} available.";

        return model;
    }
}
