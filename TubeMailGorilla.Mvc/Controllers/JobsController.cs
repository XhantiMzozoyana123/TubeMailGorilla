using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// Status hub for every background job. Pages enqueue work and redirect here
/// (or poll the JSON endpoint); the browser never waits on heavy work.
/// </summary>
[AllowAnonymous]
public class JobsController : Controller
{
    /// <summary>All recent jobs, newest first. The background-ops dashboard.</summary>
    [HttpGet]
    public IActionResult Index()
    {
        return View(JobTrackerStore.Recent(take: 30));
    }

    /// <summary>JSON status for one job - polled by page JavaScript.</summary>
    [HttpGet]
    public IActionResult Status(string id)
    {
        var job = JobTrackerStore.Get(id);
        if (job is null) return NotFound(new { error = "Unknown job." });
        return Ok(new
        {
            job.JobId,
            Kind = job.Kind.ToString(),
            State = job.State.ToString(),
            job.Label,
            job.Message,
            job.Total,
            job.Processed,
            job.Succeeded,
            job.Failed,
            job.Skipped,
            job.ElapsedLabel,
            IsActive = job.IsActive,
            Progress = Math.Round(job.ProgressFraction * 100)
        });
    }

    /// <summary>Cooperative cancel: the worker stops at its next checkpoint.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(string id, string? returnUrl)
    {
        JobCancellation.Cancel(id);
        TempData["StatusMessage"] = "Cancellation requested. The job stops at its next checkpoint.";
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}
