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

    public IActionResult Index()
    {
        return View();
    }

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
}
