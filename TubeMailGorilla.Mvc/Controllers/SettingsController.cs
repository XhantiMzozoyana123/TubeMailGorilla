using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// The Settings tab - the web twin of the MAUI Unlocked <c>SettingsPage</c>
/// (+ <c>SenderDetailsPage</c>): extraction toggles, email shortcodes,
/// SMTP accounts and the edition info.
/// </summary>
[AllowAnonymous]
public class SettingsController : Controller
{
    private readonly DatabaseService _db;

    public SettingsController(DatabaseService db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index() => View(await BuildModelAsync());

    /// <summary>EXTRACTION card - auto-posted on switch/input change.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveExtraction(bool gmailOnly, bool validateEmails, int defaultPageLimit)
    {
        SendSettings.ExtractGmailOnly = gmailOnly;
        SendSettings.ExtractValidateEmails = validateEmails;
        WebPreferences.Set("ExtractDefaultPageLimit", Math.Clamp(defaultPageLimit, 1, 50));
        TempData["StatusMessage"] = "Extraction settings saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Add a shortcode for one of the predefined lead fields.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddParameter(string field)
    {
        var existing = await _db.GetMessageParametersAsync();

        // One shortcode per data type keeps things simple - if it already
        // exists just tell the user it's ready to use.
        var already = existing.FirstOrDefault(p =>
            p.Field.Equals(field, StringComparison.OrdinalIgnoreCase));
        if (already is not null)
        {
            TempData["StatusError"] = $"That one is already in your emails as [{already.Token}].";
            return RedirectToAction(nameof(Index));
        }

        await _db.SaveMessageParameterAsync(new MessageParameter
        {
            Token = field.Replace('-', '_'),
            Field = field
        });
        TempData["StatusMessage"] = $"Shortcode added - use [{field.Replace('-', '_')}] in your emails.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Rename the selected shortcode to the typed spelling.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RenameParameter(int selectedParameterId, string customToken)
    {
        var newToken = (customToken ?? string.Empty).Trim().Trim('[', ']');
        if (selectedParameterId == 0)
        {
            TempData["StatusError"] = "Tap a shortcode in the list above to select it, then type its new spelling.";
            return RedirectToAction(nameof(Index));
        }
        if (string.IsNullOrWhiteSpace(newToken))
        {
            TempData["StatusError"] = "Type the new shortcode spelling first.";
            return RedirectToAction(nameof(Index));
        }

        var parameters = await _db.GetMessageParametersAsync();
        var selected = parameters.FirstOrDefault(p => p.Id == selectedParameterId);
        if (selected is null)
        {
            TempData["StatusError"] = "That shortcode no longer exists.";
            return RedirectToAction(nameof(Index));
        }

        selected.Token = newToken;
        await _db.SaveMessageParameterAsync(selected);
        TempData["StatusMessage"] = $"Renamed - use [{newToken}] in your emails.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Per-row delete on the shortcode list.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteParameter(int id)
    {
        await _db.DeleteMessageParameterAsync(id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>SenderDetailsPage: an empty or existing SMTP account form.</summary>
    [HttpGet]
    public async Task<IActionResult> Account(int? id)
    {
        var sender = id is > 0
            ? (await _db.GetAllSendersAsync()).FirstOrDefault(s => s.Id == id.Value)
            : null;
        return View(sender ?? new Sender());
    }

    /// <summary>Saves the SMTP account (desktop SenderDetailsPage.OnSave).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAccount(
        int id, string name, string emailAddress, string? smtpHost,
        int smtpPort, string? smtpUser, string? smtpPassword, bool isActive)
    {
        var sender = id > 0
            ? (await _db.GetAllSendersAsync()).FirstOrDefault(s => s.Id == id)
            : null;
        var isNew = sender is null;
        sender ??= new Sender();

        sender.Name = name?.Trim() ?? string.Empty;
        sender.EmailAddress = (emailAddress ?? string.Empty).Trim();
        sender.SmtpHost = string.IsNullOrWhiteSpace(smtpHost) ? null : smtpHost.Trim();
        sender.SmtpPort = smtpPort > 0 ? smtpPort : 587;
        sender.SmtpUser = string.IsNullOrWhiteSpace(smtpUser) ? null : smtpUser.Trim();
        sender.SmtpPassword = string.IsNullOrWhiteSpace(smtpPassword) ? null : smtpPassword;
        sender.IsActive = isActive;

        if (string.IsNullOrWhiteSpace(sender.EmailAddress) || string.IsNullOrWhiteSpace(sender.Name))
        {
            var model = sender;
            ViewBag.AccountError = string.IsNullOrWhiteSpace(sender.EmailAddress)
                ? "Please enter an email address."
                : "Please enter an account name.";
            return View(nameof(Account), model);
        }

        await _db.SaveSenderAsync(sender);
        TempData["StatusMessage"] = isNew ? "Account added." : "Account saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Per-row delete on the accounts list (desktop OnDeleteAccountClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        await _db.DeleteSenderAsync(id);
        TempData["StatusMessage"] = "Account removed.";
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------------ helpers

    private async Task<SettingsViewModel> BuildModelAsync()
    {
        var edition = Subscriptions.Current;
        return new SettingsViewModel
        {
            GmailOnly = SendSettings.ExtractGmailOnly,
            ValidateEmails = SendSettings.ExtractValidateEmails,
            DefaultPageLimit = WebPreferences.Get("ExtractDefaultPageLimit", 5),
            Parameters = await _db.GetMessageParametersAsync(),
            Senders = await _db.GetAllSendersAsync(),
            EditionLabel = $"Edition: {edition.Name} - every feature included",
            EditionDescription = edition.Description
        };
    }
}
