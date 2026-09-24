using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TubeMailGorilla.Mvc.Models;
using TubeMailGorilla.Mvc.Services;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// The Unlocked tab - the web twin of the MAUI Unlocked <c>SubscriptionPage</c>.
/// Static "everything is unlocked" information cards; no account, no billing.
/// </summary>
[AllowAnonymous]
public class UnlockedController : Controller
{
    /// <summary>Same copy as the desktop SubscriptionPage.</summary>
    [HttpGet]
    public IActionResult Index() => View();
}
