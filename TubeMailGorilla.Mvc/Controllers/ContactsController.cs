using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// The lead list - the web twin of the MAUI Unlocked <c>ContactsPage</c>
/// (plus <c>ContactDetailsPage</c>). Unlocked edition: anonymous and unlimited.
/// </summary>
[AllowAnonymous]
public class ContactsController : Controller
{
    private readonly DatabaseService _db;
    private readonly AIService _ai;
    private readonly ValidationService _validation;
    private readonly YouTubeTranscriptService _transcript;

    public ContactsController(
        DatabaseService db,
        AIService ai,
        ValidationService validation,
        YouTubeTranscriptService transcript)
    {
        _db = db;
        _ai = ai;
        _validation = validation;
        _transcript = transcript;
    }

    private const string SortNewestFirst = "newest";
    private const string SortNameAsc = "name";
    private const string SortEmailAsc = "email";

    /// <summary>Filter + sort happen in memory, exactly like the desktop page.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? sort) =>
        View(await BuildIndexAsync(q, sort));

    /// <summary>New contact: an empty ContactDetailsPage pushed onto the stack.</summary>
    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        var model = new ContactDetailsViewModel();
        if (id is > 0)
        {
            var contact = await _db.GetContactAsync(id.Value);
            if (contact is null) return RedirectToAction(nameof(Index));
            model.Contact = contact;
            model.Openers = await _db.GetOpenersForLeadAsync(contact.Id);
            model.Snapshots = BuildSnapshots(contact);
        }
        else
        {
            model.Contact = new EmailContact { ExtractedAt = DateTime.Now };
        }
        return View(model);
    }

    /// <summary>Save (insert or update) - the desktop page's OnSave.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id, string name, string email)
    {
        var contact = id > 0 ? await _db.GetContactAsync(id) : null;
        var isNew = contact is null;
        contact ??= new EmailContact { ExtractedAt = DateTime.Now };

        contact.Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        contact.Email = (email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(contact.Email))
        {
            var model = new ContactDetailsViewModel { Contact = contact, Error = "Please enter an email address." };
            if (!isNew) model.Openers = await _db.GetOpenersForLeadAsync(contact.Id);
            return View(nameof(Details), model);
        }

        if (isNew) await _db.AddContactAsync(contact);
        else
        {
            contact.UpdatedAt = DateTime.Now;
            await _db.UpdateContactAsync(contact);
        }
        TempData["StatusMessage"] = isNew ? "Contact added." : "Contact saved.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes one lead (row swipe / details page Delete).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? q, string? sort)
    {
        await _db.DeleteContactAsync(id);
        TempData["StatusMessage"] = "Contact deleted.";
        return RedirectToAction(nameof(Index), new { q, sort });
    }

    /// <summary>Delete All in the list header (desktop OnDeleteAllContactsClicked).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll(string? q, string? sort)
    {
        var count = (await _db.GetContactsAsync()).Count;
        if (count == 0)
        {
            TempData["StatusError"] = "You have no contacts to delete.";
            return RedirectToAction(nameof(Index), new { q, sort });
        }
        await _db.DeleteAllContactsAsync();
        TempData["StatusMessage"] = $"All {count} contacts deleted.";
        return RedirectToAction(nameof(Index), new { q, sort });
    }

    /// <summary>
    /// Bulk-generates a personalized icebreaker for every lead currently listed
    /// (after the search/sort above) - desktop OnGenerateIcebreakersClicked.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateIcebreakers(string? q, string? sort)
    {
        var model = await BuildIndexAsync(q, sort);
        if (model.Contacts.Count == 0)
        {
            TempData["StatusError"] = "Extract some leads first, then generate icebreakers.";
            return RedirectToAction(nameof(Index), new { q, sort });
        }

        // GATEKEEPER: unlocked edition always approves.
        await _validation.CheckAsync(ValidationService.GenerateIcebreaker);

        int created = 0, failed = 0, consecutiveFailures = 0;
        foreach (var contact in model.Contacts)
        {
            try
            {
                var icebreaker = await _ai.GenerateIcebreakerAsync(contact);
                if (string.IsNullOrWhiteSpace(icebreaker))
                {
                    failed++;
                    // A dead AI server fails for every remaining lead too - bail out
                    // instead of burning the full inference timeout on each one.
                    if (++consecutiveFailures >= 3) break;
                    continue;
                }

                consecutiveFailures = 0;
                await _db.SaveOpenerAsync(new Opener
                {
                    EmailerId = contact.Id,
                    Text = icebreaker,
                    CreatedAt = DateTime.Now
                });
                created++;
            }
            catch
            {
                failed++;
                if (++consecutiveFailures >= 3) break;
            }
        }

        var summary = $"Created {created} icebreaker{(created == 1 ? "" : "s")}";
        if (failed > 0) summary += $", {failed} failed (the AI service timed out or was unreachable).";
        if (consecutiveFailures >= 3) summary += " Stopped early after 3 consecutive failures.";
        TempData["StatusMessage"] = summary;
        return RedirectToAction(nameof(Index), new { q, sort });
    }

    /// <summary>Generates one icebreaker for the open contact (details page).</summary>
    /// <param name="instructions">
    /// Optional custom instructions from the user (e.g. "mention my editing style"),
    /// which supplement the lead context but never override the safety rules.
    /// </param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateIcebreaker(int id, string? instructions)
    {
        var contact = await _db.GetContactAsync(id);
        if (contact is null) return RedirectToAction(nameof(Index));

        var icebreaker = await _ai.GenerateIcebreakerAsync(contact, instructions ?? string.Empty);
        if (string.IsNullOrWhiteSpace(icebreaker))
        {
            TempData["StatusError"] = "The AI service timed out or could not be reached. Check your connection and try again.";
        }
        else
        {
            await _db.SaveOpenerAsync(new Opener
            {
                EmailerId = contact.Id,
                Text = icebreaker,
                CreatedAt = DateTime.Now
            });
            TempData["StatusMessage"] = "Icebreaker generated.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Analyses the lead's video and returns concrete editing notes aimed at
    /// raising watch time. Needs the snapshots and/or the transcript, so it is
    /// a separate opt-in action rather than part of extraction.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VideoImprovements(int id)
    {
        var contact = await _db.GetContactAsync(id);
        if (contact is null) return RedirectToAction(nameof(Index));

        string transcript = string.Empty;
        if (!string.IsNullOrWhiteSpace(contact.VideoUrl))
        {
            try
            {
                var cues = await _transcript.ExtractTranscriptCuesAsync(contact.VideoUrl);
                transcript = string.Join(" ", cues.Select(c => c.Text).Distinct());
            }
            catch
            {
                // No transcript is survivable: the analysis falls back to the
                // snapshots and the video metadata.
            }
        }

        var advice = await _ai.GenerateVideoImprovementsAsync(contact, transcript, contact.VideoSnapshot);
        if (string.IsNullOrWhiteSpace(advice))
        {
            TempData["StatusError"] = "The AI service could not produce an analysis. It may have timed out - try again.";
        }
        else
        {
            TempData["StatusMessage"] = "Analysis ready.";
        }

        // The result is shown inline, so render the details view directly
        // rather than redirecting (which would lose the text in TempData).
        var model = new ContactDetailsViewModel
        {
            Contact = contact,
            Openers = await _db.GetOpenersForLeadAsync(contact.Id),
            Snapshots = BuildSnapshots(contact),
            VideoAdvice = advice
        };
        return View(nameof(Details), model);
    }
    /// <summary>Removes one saved icebreaker from the contact details page.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOpener(int openerId, int id)
    {
        await _db.DeleteOpenerAsync(openerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>
    /// Pairs each stored snapshot with the moment it was captured at, so the
    /// view can label a frame and link back to that point in the video.
    /// </summary>
    private static List<ContactDetailsViewModel.VideoSnapshotItem> BuildSnapshots(EmailContact contact)
    {
        var images = contact.VideoSnapshot;
        var stamps = contact.VideoSnapshotTimestamps;
        var items = new List<ContactDetailsViewModel.VideoSnapshotItem>(images.Count);

        for (var i = 0; i < images.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(images[i])) continue;

            var seconds = i < stamps.Count ? stamps[i] : 0d;
            items.Add(new ContactDetailsViewModel.VideoSnapshotItem(
                images[i],
                TranscriptCue.FormatTimestamp(seconds),
                seconds,
                BuildSeekUrl(contact.VideoUrl, seconds)));
        }

        return items;
    }

    /// <summary>
    /// Appends a start offset to a YouTube watch/short URL so a frame links
    /// straight to the moment it was captured. Returns null when the URL is
    /// unusable, rather than emitting a link that goes nowhere.
    /// </summary>
    private static string? BuildSeekUrl(string? videoUrl, double seconds)
    {
        if (string.IsNullOrWhiteSpace(videoUrl)) return null;
        if (seconds <= 0) return videoUrl;

        // A bare video id needs the watch prefix; anything else is left alone
        // apart from the query separator.
        var url = videoUrl.Contains("://", StringComparison.Ordinal)
            ? videoUrl
            : "https://www.youtube.com/watch?v=" + videoUrl;

        var separator = url.Contains("?", StringComparison.Ordinal) ? "&" : "?";
        var wholeSeconds = (int)Math.Round(seconds);
        return $"{url}{separator}t={wholeSeconds}s";
    }

    /// <summary>Rebuilds the filtered/sorted list so bulk actions target
    /// exactly the rows the user is looking at.</summary>
    private async Task<ContactsViewModel> BuildIndexAsync(string? q, string? sort)
    {
        var model = new ContactsViewModel
        {
            Query = q?.Trim() ?? string.Empty,
            Sort = sort switch { SortNameAsc => 1, SortEmailAsc => 2, _ => 0 }
        };

        var contacts = await _db.GetContactsAsync();

        // GATEKEEPER: unlocked edition always approves with no limit.
        var verdict = await _validation.CheckAsync(ValidationService.ViewContacts, contacts.Count);
        if (verdict.Limit >= 0 && contacts.Count > verdict.Limit)
            contacts = contacts.Take(verdict.Limit).ToList();

        model.TotalCount = contacts.Count;

        if (model.Query.Length > 0)
        {
            contacts = contacts.Where(c =>
                (c.Name?.Contains(model.Query, StringComparison.OrdinalIgnoreCase) ?? false) ||
                c.Email.Contains(model.Query, StringComparison.OrdinalIgnoreCase) ||
                (c.Channel?.Contains(model.Query, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }

        model.Contacts = SortContacts(contacts, model.Sort).ToList();
        return model;
    }

    private static IEnumerable<EmailContact> SortContacts(IEnumerable<EmailContact> contacts, int sort) =>
        sort switch
        {
            // Unnamed contacts sink to the bottom, then A-Z by name.
            1 => contacts.OrderBy(c => string.IsNullOrWhiteSpace(c.Name) ? 1 : 0)
                         .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase),
            2 => contacts.OrderBy(c => c.Email, StringComparer.OrdinalIgnoreCase),
            // Default: newest first, so a lead that was just scraped is on top.
            _ => contacts.OrderByDescending(c => c.ExtractedAt).ThenByDescending(c => c.Id)
        };
}

