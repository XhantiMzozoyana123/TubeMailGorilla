using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// The marketing home page + public pricing page, rendered server-side.
/// Reads the same plan catalog configuration as the API.
/// </summary>
public class HomeController : Controller
{
    private readonly SubscriptionPlansOptions _plans;
    private readonly FreePlanLimits _freePlan;

    public HomeController(IOptions<SubscriptionPlansOptions> plansOptions, IOptions<FreePlanLimits> freePlanOptions)
    {
        _plans = plansOptions.Value;
        _freePlan = freePlanOptions.Value;
    }

    /// <summary>
    /// The web edition opens straight into the app, like the desktop build
    /// landing on its first tab (Extract) - there is no marketing home.
    /// </summary>
    public IActionResult Index() => RedirectToAction(nameof(ExtractController.Index), "Extract");

    /// <summary>Public pricing page (plan catalog from configuration).</summary>
    [HttpGet("/pricing")]
    public IActionResult Pricing()
    {
        var model = new PricingViewModel
        {
            Plans = _plans.Plans.Where(p => p.IsEnabled).ToList(),
            FreeLeadsPerExtraction = _freePlan.MaxLeadsPerExtraction,
            FreeContactsVisible = _freePlan.MaxContactsVisible,
            FreeEmailsPerCampaign = _freePlan.MaxEmailsPerCampaign
        };
        return View(model);
    }

    /// <summary>
    /// Generic error page - the target of UseExceptionHandler("/Home/Error")
    /// in production. Without it, any unhandled exception causes a second
    /// failure while trying to render the (non-existent) error page.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [HttpGet("/Home/Error")]
    public IActionResult Error()
    {
        return View();
    }
}
