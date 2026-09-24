using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// Email template manager - the web twin of the MAUI Unlocked
/// <c>EmailTemplatesPage</c> + <c>EmailTemplateDetailsPage</c>.
/// Unlocked edition: templates are always available, no gate.
/// </summary>
[AllowAnonymous]
public class TemplatesController : Controller
{
    private readonly DatabaseService _db;

    public TemplatesController(DatabaseService db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var templates = await _db.GetTemplatesAsync();
        return View(templates.OrderBy(t => t.Name).ToList());
    }

    /// <summary>Template editor - an empty or existing EmailTemplateDetailsPage.</summary>
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        var template = id is > 0
            ? (await _db.GetTemplatesAsync()).FirstOrDefault(t => t.Id == id.Value)
            : null;
        return View(new TemplateDetailsViewModel { Template = template ?? new EmailTemplate() });
    }

    /// <summary>Saves the template (desktop EmailTemplateDetailsPage.OnSave).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string name, string subject, string body)
    {
        var template = id > 0
            ? (await _db.GetTemplatesAsync()).FirstOrDefault(t => t.Id == id)
            : null;
        var isNew = template is null;
        template ??= new EmailTemplate();

        template.Name = (name ?? string.Empty).Trim();
        template.Subject = (subject ?? string.Empty).Trim();
        template.Body = body ?? string.Empty;

        if (string.IsNullOrWhiteSpace(template.Name))
        {
            return View(nameof(Details), new TemplateDetailsViewModel
            {
                Template = template,
                Error = "Please enter a template name."
            });
        }

        await _db.SaveTemplateAsync(template);
        TempData["StatusMessage"] = isNew ? "Template created." : "Template saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes a template (desktop OnDeleteTemplateClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.DeleteTemplateAsync(id);
        TempData["StatusMessage"] = "Template deleted.";
        return RedirectToAction(nameof(Index));
    }
}
