using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// Lead extraction - the web twin of the MAUI Unlocked <c>ExtractPage</c>.
///
/// Unlocked edition: no sign-in and no paywall. <see cref="ValidationService"/>
/// approves everything locally, exactly like the desktop app, so this page is
/// anonymous and unlimited.
/// </summary>
[AllowAnonymous]
public class ExtractController : Controller
{
    private readonly ExtractService _extract;
    private readonly DatabaseService _db;
    private readonly LLMService _llm;
    private readonly ValidationService _validation;
    private readonly PaymentService _payments;

    public ExtractController(
        ExtractService extract,
        DatabaseService db,
        LLMService llm,
        ValidationService validation,
        PaymentService payments)
    {
        _extract = extract;
        _db = db;
        _llm = llm;
        _validation = validation;
        _payments = payments;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new ExtractViewModel
        {
            PageViewLimit = WebPreferences.Get("ExtractDefaultPageLimit", 5),
            GmailOnly = SendSettings.ExtractGmailOnly,
            ValidateEmails = SendSettings.ExtractValidateEmails
        };
        await PopulateAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run(string? keyword, int pageViewLimit)
    {
        // Desktop ExtractPage reads these from settings (the Settings tab owns them).
        var gmailOnly = SendSettings.ExtractGmailOnly;
        var validateEmails = SendSettings.ExtractValidateEmails;

        var model = new ExtractViewModel
        {
            Keyword = keyword,
            PageViewLimit = pageViewLimit <= 0 ? 5 : pageViewLimit,
            GmailOnly = gmailOnly,
            ValidateEmails = validateEmails
        };

        if (string.IsNullOrWhiteSpace(keyword))
        {
            model.StatusMessage = "Please enter a keyword.";
            await PopulateAsync(model);
            return View(nameof(Index), model);
        }

        // The unlocked edition has no gatekeeper - the call is kept so the page
        // matches the desktop flow, and it always approves.
        await _validation.CheckAsync(ValidationService.ExtractLeads, model.PageViewLimit);

        // Report Ollama connectivity up front, exactly like the desktop page.
        await _llm.EnsureReadyAsync();
        model.LlmStatus = _llm.Status;

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _extract.ExtractByKeywordAsync(
                keyword.Trim(), model.PageViewLimit, gmailOnly, validateEmails);

            model.TotalVideos = result.TotalVideos;
            model.EmailsFound = result.EmailsFound;
            model.Errors = result.Errors;
            model.StatusMessage = "Done.";
        }
        catch (Exception ex)
        {
            model.Error = $"Extraction failed: {ex.Message}";
        }

        model.Elapsed = stopwatch.Elapsed;
        await PopulateAsync(model);
        return View(nameof(Index), model);
    }

    /// <summary>Removes a lead from the local store (Contacts page keeps the list).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.DeleteContactAsync(id);
        TempData["StatusMessage"] = "Lead deleted.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Bulk CSV template download (desktop OnDownloadTemplateClicked).</summary>
    [HttpGet]
    public IActionResult BulkTemplate()
    {
        var template =
            "keyword,pages\n" +
            "marketing,5\n" +
            "tech review,3\n" +
            "\"video editing services\",5\n" +
            "fitness coach,2\n";
        return File(System.Text.Encoding.UTF8.GetBytes(template), "text/csv",
            $"TubeMailGorilla-Bulk-Template-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
    }

    /// <summary>Bulk extraction from an uploaded CSV (desktop OnBulkExtractionClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk(IFormFile? file)
    {
        var model = new ExtractViewModel
        {
            PageViewLimit = WebPreferences.Get("ExtractDefaultPageLimit", 5),
            GmailOnly = SendSettings.ExtractGmailOnly,
            ValidateEmails = SendSettings.ExtractValidateEmails
        };
        await PopulateAsync(model);

        if (file is null || file.Length == 0)
        {
            model.BulkStatusMessage = "Pick a CSV file first — or tap “Download CSV Bulk Extraction Template” for a ready-made one.";
            return View(nameof(Index), model);
        }

        string content;
        using (var reader = new StreamReader(file.OpenReadStream()))
            content = await reader.ReadToEndAsync();

        List<(string Keyword, int Pages)> rows;
        try
        {
            rows = ParseBulkCsv(content);
        }
        catch (Exception ex)
        {
            model.BulkStatusMessage = $"Could not read CSV: {ex.Message}";
            return View(nameof(Index), model);
        }

        if (rows.Count == 0)
        {
            model.BulkStatusMessage =
                "That file came back empty.\n\nWe couldn't find any keywords in it.\n\nIt should look like this:\n\nmarketing,5\ntech review,3\n\nTip: tap “Download CSV Bulk Extraction Template” first — it gives you a ready-made file to fill in.";
            return View(nameof(Index), model);
        }

        // GATEKEEPER: bulk extraction is approved before any work begins.
        await _validation.CheckAsync(ValidationService.BulkExtractLeads, 0);
        await _llm.EnsureReadyAsync();
        model.LlmStatus = _llm.Status;

        var totalVideos = 0;
        var totalEmails = 0;
        var totalErrors = 0;

        try
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                await _validation.CheckAsync(ValidationService.ExtractLeads, row.Pages);

                var result = await _extract.ExtractByKeywordAsync(
                    row.Keyword, row.Pages,
                    SendSettings.ExtractGmailOnly, SendSettings.ExtractValidateEmails);

                totalVideos += result.TotalVideos;
                totalEmails += result.EmailsFound;
                totalErrors += result.Errors;
            }

            model.TotalVideos = totalVideos;
            model.EmailsFound = totalEmails;
            model.Errors = totalErrors;
            model.BulkStatusMessage =
                $"Bulk extract complete.\nKeywords run: {rows.Count}/{rows.Count}\nTotal videos: {totalVideos}  |  Total emails: {totalEmails}  |  Errors: {totalErrors}" +
                (totalEmails > 0 ? "\n\nYour fresh leads are waiting on the Contacts page." : "");
        }
        catch (Exception ex)
        {
            model.BulkStatusMessage =
                $"The Gorilla tripped over something: {ex.Message}\n—\nProgress so far — Videos: {totalVideos}, Emails: {totalEmails}.";
        }

        await PopulateAsync(model);
        return View(nameof(Index), model);
    }

    /// <summary>
    /// Parses the bulk CSV. Accepts an optional header row and both
    /// "keyword,pages" and bare "keyword" lines (pages defaults to 5).
    /// Handles double-quoted keywords containing commas; '#' lines are comments.
    /// </summary>
    private static List<(string Keyword, int Pages)> ParseBulkCsv(string content)
    {
        var rows = new List<(string, int)>();

        foreach (var rawLine in content.Split('\n'))
        {
            var line = rawLine.Trim().TrimEnd('\r');
            if (line.Length == 0 || line.StartsWith('#')) continue;

            var fields = SplitCsvLine(line);
            if (fields.Length == 0) continue;

            var keyword = fields[0].Trim();
            if (keyword.Length == 0) continue;

            // Skip a header row like "keyword,pages".
            if (rows.Count == 0 && keyword.Equals("keyword", StringComparison.OrdinalIgnoreCase))
                continue;

            var pages = 5;
            if (fields.Length > 1 && int.TryParse(fields[1].Trim(), out var parsed) && parsed > 0)
                pages = parsed;

            rows.Add((keyword, pages));
        }

        return rows;
    }

    /// <summary>Splits one CSV line, honoring double-quoted fields.</summary>
    private static string[] SplitCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());

        return fields.ToArray();
    }

    private async Task PopulateAsync(ExtractViewModel model)
    {
        model.Entitlements = await _payments.GetEntitlementsAsync();
        model.Contacts = await _db.GetContactsAsync();
        model.YtDlpAvailable = YtDlp.IsAvailable;
    }
}
