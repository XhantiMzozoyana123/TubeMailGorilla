using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Constants;

namespace TubeMailGorilla.Api.Controllers;

/// <summary>
/// Presentation-layer controller that exposes PayPal payment endpoints.
/// Thin orchestration layer: receives HTTP requests, delegates to the
/// Application-layer <see cref="ISubscriptionService"/> use-case, and
/// translates results into HTTP responses.
///
/// After a successful capture, the subscription claim ("subscription": "active")
/// is persisted in the user's Identity record. On the next login the JWT will
/// carry that claim and [Authorize(Policy = "Subscribed")] will pass.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
        private readonly ISubscriptionService _subscriptionService;
    private readonly PricingSettings _pricing;
    private readonly SubscriptionPlansOptions _plans;
    private readonly FreePlanLimits _freePlan;

    public PaymentsController(
        ISubscriptionService subscriptionService,
        Microsoft.Extensions.Options.IOptions<PricingSettings> pricingOptions,
        Microsoft.Extensions.Options.IOptions<SubscriptionPlansOptions> plansOptions,
        Microsoft.Extensions.Options.IOptions<FreePlanLimits> freePlanOptions)
    {
        _subscriptionService = subscriptionService;
        _pricing = pricingOptions.Value;
        _plans = plansOptions.Value;
        _freePlan = freePlanOptions.Value;
    }

    /// <summary>
    /// The subscription plans users can buy, in plain language (name, price,
    /// benefits). This is what the website and desktop app display.
    /// Contract: GET /api/payments/plans  (anonymous)
    /// </summary>
    [HttpGet("plans")]
    [AllowAnonymous]
    public IActionResult GetPlans()
    {
        var plans = _plans.Plans
            .Where(p => p.IsEnabled)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Tagline,
                MonthlyPrice = p.MonthlyPrice,
                p.Currency,
                p.LeadsPerMonth,
                Features = p.Features
            });

        return Ok(plans);
    }

    /// <summary>
    /// Returns the server-configured subscription price so clients can
    /// display it without hardcoding amounts. Derived from the plan catalog.
    /// Contract: GET /api/payments/pricing
    /// </summary>
    [HttpGet("pricing")]
    [AllowAnonymous]
    public IActionResult GetPricing()
    {
        var defaultPlan = _plans.DefaultPlan;
        if (defaultPlan is not null)
        {
            return Ok(new { Amount = defaultPlan.MonthlyPrice, Currency = defaultPlan.Currency });
        }

        return Ok(new { Amount = _pricing.Amount, Currency = _pricing.Currency });
    }

    /// <summary>
    /// Creates a PayPal payment order and returns the approval URL.
    /// The client should redirect the user to <see cref="PaymentResponse.ApprovalUrl"/>.
    /// Contract: POST /api/payments/create  { Amount, Currency? }
    /// </summary>
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
    {
        // The authenticated caller owns the order; client-sent amounts are ignored.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found in token.");

        var requestWithUser = request with { UserId = userId };
        var response = await _subscriptionService.CreatePaymentAsync(requestWithUser);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Captures an approved PayPal order. On success the subscription claim
    /// is awarded to the current user.
    /// Contract: POST /api/payments/capture  { OrderId }
    /// </summary>
    [HttpPost("capture")]
    [Authorize]
    public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found in token.");

        var response = await _subscriptionService.CapturePaymentAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Returns whether the current user has an active subscription claim.
    /// Checked against the persisted Identity claim store (AspNetUserClaims),
    /// so a just-captured payment is reflected immediately without re-login.
    /// Contract: GET /api/payments/status
    /// </summary>
        [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetSubscriptionStatus()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found in token.");

        // Single, authoritative subscription state shared by the website and
        // the desktop app: plan name, price and next billing date included.
        var status = await _subscriptionService.GetStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Demonstrates an endpoint protected by the subscription claim.
    /// Any controller or action decorated with [Authorize(Policy = "Subscribed")]
    /// will only be reachable by users whose JWT carries the subscription claim.
    /// Contract: GET /api/payments/premium
    /// </summary>
    [HttpGet("premium")]
    [Authorize(Policy = "Subscribed")]
    public IActionResult PremiumFeature()
    {
        return Ok(new { Message = "You have access to premium features." });
    }

    /// <summary>
    /// What the current user's plan allows. The single source of truth for all
    /// feature gating in the desktop app: extraction limits, how many contacts
    /// are visible, campaign size, and whether Icebreakers / Email Templates /
    /// the Contact Blocklist are unlocked. Paying users get the paid plan's
    /// limits; everyone else gets the configured FREE plan limits.
    /// Contract: GET /api/payments/entitlements
    /// </summary>
    [HttpGet("entitlements")]
    [Authorize]
    public async Task<IActionResult> GetEntitlements()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found in token.");

        var status = await _subscriptionService.GetStatusAsync(userId);

        if (status.IsSubscribed)
        {
            var plan = status.PlanId is not null
                ? _plans.Plans.FirstOrDefault(p => p.Id == status.PlanId) ?? _plans.DefaultPlan
                : _plans.DefaultPlan;

            return Ok(new EntitlementsDto(
                IsSubscribed: true,
                PlanId: plan?.Id ?? "pro",
                PlanName: plan?.Name ?? "Pro",
                MaxLeadsPerExtraction: plan?.LeadsPerMonth ?? 5000,
                MaxContactsVisible: -1,
                MaxEmailsPerCampaign: -1,
                IcebreakerEnabled: true,
                EmailTemplatesEnabled: true,
                BlocklistEnabled: true));
        }

        return Ok(new EntitlementsDto(
            IsSubscribed: false,
            PlanId: "free",
            PlanName: "Free",
            // Free users aren't capped per extraction (unlimited) - they're
            // throttled to ONE extraction per month server-side instead.
            MaxLeadsPerExtraction: -1,
            MaxContactsVisible: _freePlan.MaxContactsVisible,
            MaxEmailsPerCampaign: _freePlan.MaxEmailsPerCampaign,
            IcebreakerEnabled: false,
            EmailTemplatesEnabled: false,
            BlocklistEnabled: false));
    }

    /// <summary>
    /// Feature gate for the Block Contact (blocklist) page in the desktop app.
    /// The [Authorize(Policy = "Subscribed")] attribute does all the work:
    /// paying users pass (200), free users get 403 Forbidden before this code runs.
    /// The desktop app calls this once when opening the blocklist page and treats
    /// any non-success status as "locked".
    /// Contract: GET /api/payments/features/blocklist
    /// </summary>
    [HttpGet("features/blocklist")]
    [Authorize(Policy = "Subscribed")]
    public IActionResult GetBlocklistAccess()
    {
        return Ok(new { Feature = "blocklist", Allowed = true });
    }

    /// <summary>
    /// Cancels the current user's subscription. Removes the persisted
    /// active-subscription claim and returns a fresh JWT without it, so
    /// premium endpoints reject immediately.
    /// Contract: POST /api/payments/cancel
    /// </summary>
    [HttpPost("cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found in token.");

        var response = await _subscriptionService.CancelSubscriptionAsync(userId);
        return Ok(response);
    }
}
