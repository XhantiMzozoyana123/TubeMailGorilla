using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// The blocklist manager - the web twin of the MAUI Unlocked <c>BlockedPage</c>.
/// Unlocked edition: the blocklist is always available, no gate.
/// </summary>
[AllowAnonymous]
public class BlockedController : Controller
{
    private readonly DatabaseService _db;

    public BlockedController(DatabaseService db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new BlockedViewModel
        {
            Blockers = await _db.GetBlockersAsync()
        };
        return View(model);
    }

    /// <summary>Adds an email to the blocklist (desktop OnAddBlockerClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(string email)
    {
        var trimmed = (email ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            TempData["StatusError"] = "Please enter an email address to block.";
            return RedirectToAction(nameof(Index));
        }

        await _db.AddBlockerAsync(new Blocker { BlockedEmail = trimmed, CreatedAt = DateTime.Now });
        TempData["StatusMessage"] = $"{trimmed} added to the blocklist.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Removes an email from the blocklist (desktop OnDeleteBlockerClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id)
    {
        await _db.RemoveBlockerAsync(id);
        TempData["StatusMessage"] = "Email removed from the blocklist.";
        return RedirectToAction(nameof(Index));
    }
}
