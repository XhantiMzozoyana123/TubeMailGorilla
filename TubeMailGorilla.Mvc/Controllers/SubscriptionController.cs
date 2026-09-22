using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Infrastructure.Models;
using TubeMailGorilla.Mvc.Models;

namespace TubeMailGorilla.Mvc.Controllers;

/// <summary>
/// Subscription management for the signed-in user: current status, PayPal
/// upgrade redirect, capture-after-approval, and cancellation. Uses the same
/// Application-layer <see cref="ISubscriptionService"/> as the API so the
/// website and desktop app share one subscription state.
/// </summary>
[Authorize]
public class SubscriptionController : Controller
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly SubscriptionPlansOptions _plans;
    private readonly FreePlanLimits _freePlan;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SubscriptionController(
        ISubscriptionService subscriptionService,
        IOptions<SubscriptionPlansOptions> plansOptions,
        IOptions<FreePlanLimits> freePlanOptions,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _subscriptionService = subscriptionService;
        _plans = plansOptions.Value;
        _freePlan = freePlanOptions.Value;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    /// <summary>Current plan, price and next billing date for the signed-in user.</summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var status = await _subscriptionService.GetStatusAsync(UserId);
        return View(new SubscriptionIndexViewModel(status, BuildEntitlements(status)));
    }

    /// <summary>
    /// Starts a PayPal subscription checkout and redirects the browser to the
    /// approval URL. PayPal then returns the buyer to Subscription/Return.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upgrade()
    {
        var siteRoot = $"{Request.Scheme}://{Request.Host}";
        var response = await _subscriptionService.CreatePaymentAsync(new PaymentRequest
        {
            ReturnUrl = $"{siteRoot}/Subscription/Return",
            CancelUrl = $"{siteRoot}/Subscription/Index",
            UserId = UserId
        });

        if (!response.Success || string.IsNullOrEmpty(response.ApprovalUrl))
        {
            TempData["StatusError"] = response.Message ?? "Could not start the PayPal checkout. Please try again.";
            return RedirectToAction(nameof(Index));
        }

        return Redirect(response.ApprovalUrl);
    }

    /// <summary>
    /// PayPal redirect target after the buyer approves. Verifies the subscription
    /// at PayPal and awards the persisted subscription claim (mirrors the API's
    /// POST /api/payments/capture).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Return(string? subscription_id, string? token)
    {
        if (string.IsNullOrEmpty(subscription_id))
        {
            TempData["StatusError"] = "The PayPal approval did not complete. No charge was made - please try again.";
            return RedirectToAction(nameof(Index));
        }

        var response = await _subscriptionService.CapturePaymentAsync(new CapturePaymentRequest(subscription_id), UserId);
        if (!response.Success)
        {
            TempData["StatusError"] = response.Message ?? "Could not activate the subscription.";
            return RedirectToAction(nameof(Index));
        }

        // The subscription claim changed - refresh the cookie principal so
        // [Authorize(Policy = "Subscribed")] passes on the very next request.
        await RefreshSignInAsync();

        TempData["StatusMessage"] = response.Message ?? "Your Pro subscription is active.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Cancels at PayPal first, then removes the local claim (identical safety
    /// rules to the API's POST /api/payments/cancel).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel()
    {
        var response = await _subscriptionService.CancelSubscriptionAsync(UserId);
        await RefreshSignInAsync();

        if (!response.Success)
            TempData["StatusError"] = response.Message ?? "Could not cancel right now - you have NOT been charged further.";
        else
            TempData["StatusMessage"] = response.Message ?? "Subscription cancelled.";

        return RedirectToAction(nameof(Index));
    }

    private EntitlementsViewModel BuildEntitlements(SubscriptionStatusResponse status)
    {
        if (status.IsSubscribed)
        {
            var plan = _plans.Plans.FirstOrDefault(p => p.Id == status.PlanId) ?? _plans.DefaultPlan;
            return new EntitlementsViewModel
            {
                IsSubscribed = true,
                PlanId = plan?.Id ?? "pro",
                PlanName = plan?.Name ?? "Pro",
                MaxLeadsPerExtraction = plan?.LeadsPerMonth ?? 5000,
                MaxContactsVisible = -1,
                MaxEmailsPerCampaign = -1,
                IcebreakerEnabled = true,
                EmailTemplatesEnabled = true,
                BlocklistEnabled = true
            };
        }

        return new EntitlementsViewModel
        {
            IsSubscribed = false,
            PlanId = "free",
            PlanName = "Free",
            MaxLeadsPerExtraction = _freePlan.MaxLeadsPerExtraction,
            MaxContactsVisible = _freePlan.MaxContactsVisible,
            MaxEmailsPerCampaign = _freePlan.MaxEmailsPerCampaign,
            IcebreakerEnabled = false,
            EmailTemplatesEnabled = false,
            BlocklistEnabled = false
        };
    }

    /// <summary>Rebuilds the cookie principal from the claim store after changes.</summary>
    private async Task RefreshSignInAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is not null)
            await _signInManager.RefreshSignInAsync(user);
    }
}
