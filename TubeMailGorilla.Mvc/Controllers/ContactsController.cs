using System.IO.Compression;
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
    private readonly BackgroundJobService _jobs;

    public ContactsController(
        DatabaseService db,
        AIService ai,
        ValidationService validation,
        YouTubeTranscriptService transcript,
        BackgroundJobService jobs)
    {
        _db = db;
        _ai = ai;
        _validation = validation;
        _transcript = transcript;
        _jobs = jobs;
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
    /// Bulk-generates an icebreaker per lead on a worker. Per-lead try/catch
    /// plus a 3-consecutive-failure circuit breaker: a dead AI server stops the
    /// batch instead of burning a 300s timeout on each remaining lead.
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

        // Single-flight: two AI batches would contend on the Ollama server.
        if (JobTrackerStore.HasActive(JobKind.Icebreakers))
        {
            TempData["StatusError"] = "An AI batch is already running. Track it on the Jobs page.";
            return RedirectToAction(nameof(Index), new { q, sort });
        }

        try
        {
            var ids = model.Contacts.Select(c => c.Id).ToList();
            _jobs.EnqueueIcebreakers(ids, q ?? string.Empty);
            TempData["StatusMessage"] =
                $"Icebreakers queued for {ids.Count} leads. You can close the browser - progress is on the Jobs page.";
            return RedirectToAction("Index", "Jobs");
        }
        catch (Exception ex)
        {
            TempData["StatusError"] = $"Could not queue the icebreaker batch: {ex.Message}";
        }

        return RedirectToAction(nameof(Index), new { q, sort });
    }

    /// <summary>
    /// Generates one icebreaker for the open contact (details page) as a
    /// background job, so a slow Ollama call never blocks the page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateIcebreaker(int id, string? instructions)
    {
        var contact = await _db.GetContactAsync(id);
        if (contact is null) return RedirectToAction(nameof(Index));

        var queued = _jobs.EnqueueIcebreakers(new List<int> { contact.Id }, string.Empty);
        TempData["StatusMessage"] =
            $"Icebreaker queued. Track it on the Jobs page - it will appear on the Details page when ready.";
        return RedirectToAction("Index", "Jobs");
    }

    /// <summary>
    /// Analyses the lead's video and returns concrete editing notes aimed at
    /// raising watch time. Never runs inline - the transcript fetch (yt-dlp,
    /// 45s cap) and the vision call can each take minutes, so it runs as a
    /// worker job whose result lives in VideoReviewStore (survives the request).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VideoImprovements(int id)
    {
        var contact = await _db.GetContactAsync(id);
        if (contact is null) return RedirectToAction(nameof(Index));

        var queued = _jobs.EnqueueVideoReview(id, contact.Email);
        TempData["StatusMessage"] =
            $"Video analysis queued. Track it on the Jobs page - your review will appear on the Details page when ready.";
        return RedirectToAction("Index", "Jobs");
    }

    /// <summary>Removes one saved icebreaker from the contact details page.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOpener(int openerId, int id)
    {
        await _db.DeleteOpenerAsync(openerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Streams the lead's video snapshots as a single ZIP archive.
    ///
    /// A GET rather than a POST because it is a download, not a state change -
    /// that way the browser handles it natively (progress, save dialog) with no
    /// JavaScript. Alongside the frames it includes a small manifest so the
    /// archive is self-describing once it leaves the site: which timestamp each
    /// frame came from, and the source video seek link.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> DownloadSnapshots(int id)
    {
        var contact = await _db.GetContactAsync(id);
        if (contact is null) return RedirectToAction(nameof(Index));

        var images = contact.VideoSnapshot;
        if (images.Count == 0)
        {
            TempData["StatusError"] = "This lead has no snapshots to download.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var stamps = contact.VideoSnapshotTimestamps;

        // NOT disposed here: MVC reads this stream after the action returns, so
        // disposing it produced "ObjectDisposedException: Cannot access a closed
        // Stream" and a 500 on every download. It is left to the GC.
        var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            var written = 0;

            for (var i = 0; i < images.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(images[i])) continue;

                var seconds = i < stamps.Count ? stamps[i] : 0d;
                var entry = archive.CreateEntry(
                    // Colons are illegal in filenames on Windows and need escaping in
                    // zip entry names, so the hh:mm:ss label is flattened.
                    string.Format("snapshot_{0:D2}_{1}.jpg", i, Flatten(TranscriptCue.FormatTimestamp(seconds))),
                    CompressionLevel.Fastest);

                byte[] jpeg;
                try
                {
                    // Stored as bare base64, but tolerate a data-URI prefix.
                    var payload = images[i].StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                        ? images[i][(images[i].IndexOf(',') + 1)..]
                        : images[i];
                    jpeg = Convert.FromBase64String(payload);
                }
                catch
                {
                    // A single corrupt frame must not sink the whole download.
                    continue;
                }

                await using var stream = entry.Open();
                await stream.WriteAsync(jpeg);
                written++;
            }

            if (written == 0)
            {
                TempData["StatusError"] = "None of the stored snapshots could be read.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Manifest: makes the archive useful without the web UI alongside it.
            var manifest = archive.CreateEntry("README.txt", CompressionLevel.Fastest);
            await using var manifestStream = manifest.Open();
            await using var writer = new StreamWriter(manifestStream);

            await writer.WriteLineAsync("TubeMailGorilla - lead video snapshots");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync($"Lead:     {contact.DisplayName} <{contact.Email}>");
            await writer.WriteLineAsync($"Channel:  {contact.Channel}");
            await writer.WriteLineAsync($"Video:    {contact.VideoTitle}");
            await writer.WriteLineAsync($"Captured: {contact.ExtractedAt:g}");
            if (!string.IsNullOrWhiteSpace(contact.VideoUrl))
                await writer.WriteLineAsync($"Source:   {BuildSeekUrl(contact.VideoUrl, 0)}");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync("Frames (one per 10 seconds of the video):");
            for (var i = 0; i < written; i++)
            {
                var seconds = i < stamps.Count ? stamps[i] : 0d;
                await writer.WriteLineAsync(
                    $"  snapshot_{i:D2}_{Flatten(TranscriptCue.FormatTimestamp(seconds))}.jpg");
            }
        }

        buffer.Position = 0;
        var fileName = BuildZipFileName(contact);
        return File(buffer, "application/zip", fileName);
    }

    /// <summary>
    /// A safe, readable download name. Channel names come from scraped YouTube
    /// metadata, so anything that is illegal in a filename has to go - an
    /// unfiltered value ends up in a Content-Disposition header.
    /// </summary>
    /// <summary>"00:01:24" -> "00-01-24", for use inside a filename.</summary>
    private static string Flatten(string timestamp) => timestamp.Replace(':', '-');

    private static string BuildZipFileName(EmailContact contact)
    {
        var source = !string.IsNullOrWhiteSpace(contact.Channel)
            ? contact.Channel
            : contact.Email;

        var safe = new string(source
            .Select(ch => char.IsLetterOrDigit(ch) || ch is ' ' or '-' or '_' ? ch : '-')
            .ToArray())
            .Trim('-', ' ');

        if (safe.Length == 0) safe = "lead";
        if (safe.Length > 60) safe = safe[..60].Trim('-', ' ');

        return $"{safe}-snapshots.zip";
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

