using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain;

namespace TubeMailGorilla.Api.Controllers;

/// <summary>
/// Gatekeeper: every workload the desktop app wants to perform (extraction,
/// sending emails, viewing contacts, AI icebreakers, email templates, the
/// contact blocklist) must be approved here FIRST. The client never decides
/// what its plan allows - it asks, and only proceeds on an explicit green
/// light. Unknown actions and tampered amounts are denied.
///
/// All authorization logic lives server-side; the app is untrusted input.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ValidationController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IExtractionUsageService _extractionUsageService;
    private readonly SubscriptionPlansOptions _plans;
    private readonly FreePlanLimits _freePlan;

    public ValidationController(
        ISubscriptionService subscriptionService,
        IExtractionUsageService extractionUsageService,
        Microsoft.Extensions.Options.IOptions<SubscriptionPlansOptions> plansOptions,
        Microsoft.Extensions.Options.IOptions<FreePlanLimits> freePlanOptions)
    {
        _subscriptionService = subscriptionService;
        _extractionUsageService = extractionUsageService;
        _plans = plansOptions.Value;
        _freePlan = freePlanOptions.Value;
    }

    /// <summary>
    /// Pre-flight check. Contract:
    /// POST /api/validation/check  { Action, RequestedAmount? }
    /// 200 with Approved=false is still a "successful" call - the verdict is
    /// in the body so clients can show the upgrade message verbatim.
    /// </summary>
    [HttpPost("check")]
    public async Task<IActionResult> Check([FromBody] ValidationRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User identifier not found in token.");

        // Whitelist first: anything unknown is denied outright (no plan can buy it).
        if (request.Action is null || !ValidationAction.All.Contains(request.Action))
        {
            return Ok(new ValidationResponse(false, request.Action ?? "",
                "Unknown or unsupported action.", "free", "Free", 0, request.RequestedAmount));
        }

        var status = await _subscriptionService.GetStatusAsync(userId);
        var planId = status.IsSubscribed ? status.PlanId : "free";
        var planName = status.IsSubscribed ? status.PlanName : "Free";

        // Resolve the caller's limits from their plan.
        var maxLeads = int.MaxValue;
        var maxContactsVisible = -1;
        var maxEmailsPerCampaign = -1;

        if (status.IsSubscribed)
        {
            var plan = !string.IsNullOrEmpty(status.PlanId)
                ? _plans.Plans.FirstOrDefault(p => p.Id == status.PlanId) ?? _plans.DefaultPlan
                : _plans.DefaultPlan;
            maxLeads = plan?.LeadsPerMonth ?? 5000;
        }
        else
        {
            // Free (non-paying) users are capped to a small number of leads per
            // extraction run (FreePlan:MaxLeadsPerExtraction) AND throttled to
            // exactly ONE extraction per calendar month. Both guards are enforced
            // in the ExtractLeads case below. Contacts & sends keep their own limits.
            maxLeads = _freePlan.MaxLeadsPerExtraction;
            maxContactsVisible = _freePlan.MaxContactsVisible;
            maxEmailsPerCampaign = _freePlan.MaxEmailsPerCampaign;
        }

        bool featureEnabled = status.IsSubscribed; // icebreakers/templates/blocklist are all-or-nothing

        ValidationResponse Deny(string reason) => new(
            false, request.Action, reason, planId, planName,
            LimitFor(request.Action, maxLeads, maxContactsVisible, maxEmailsPerCampaign),
            request.RequestedAmount);

        ValidationResponse Approve() => new(
            true, request.Action, "Approved. You may continue with your workload.",
            planId, planName,
            LimitFor(request.Action, maxLeads, maxContactsVisible, maxEmailsPerCampaign),
            request.RequestedAmount);

        switch (request.Action)
        {
            case ValidationAction.ExtractLeads:

                // Free (non-paying) users get exactly ONE extraction per calendar
                // month. The server enforces this (records the run when approved),
                // so no client-side tampering can grant more runs. Paying users are
                // unlimited by quota and only bound by the per-extraction lead cap.
                if (!status.IsSubscribed)
                {
                    if (!await _extractionUsageService.TryConsumeFreeExtractionAsync(userId))
                    {
                        return Ok(Deny("You've used your free extraction for this month. Please upgrade to Pro to keep extracting, or wait until next month for another free run."));
                    }
                }

                if (maxLeads > 0)
                {
                    var amount = request.RequestedAmount > 0 ? request.RequestedAmount : maxLeads;
                    if (amount > maxLeads)
                        return Ok(Deny($"{planName} plan allows up to {maxLeads} leads per extraction. Please upgrade your subscription for more."));
                }
                break;

            case ValidationAction.BulkExtractLeads:

                // Bulk extraction is a Pro feature; free users are blocked outright.
                if (!status.IsSubscribed)
                    return Ok(Deny("Bulk extraction is a Pro feature. Please upgrade your subscription to unlock it."));
                break;

            case ValidationAction.SendEmails:
                if (maxEmailsPerCampaign >= 0)
                {
                    var recipients = request.RequestedAmount > 0 ? request.RequestedAmount : maxEmailsPerCampaign;
                    if (recipients > maxEmailsPerCampaign)
                        return Ok(Deny($"{planName} plan allows campaigns of up to {maxEmailsPerCampaign} emails. Please upgrade your subscription for unlimited sends."));
                }
                break;

            case ValidationAction.ViewContacts:
                if (maxContactsVisible >= 0)
                {
                    var wanted = request.RequestedAmount > 0 ? request.RequestedAmount : maxContactsVisible;
                    if (wanted > maxContactsVisible)
                        return Ok(Deny($"{planName} plan shows up to {maxContactsVisible} contacts. Please upgrade your subscription to see all of them."));
                }
                break;

            case ValidationAction.GenerateIcebreaker:
                if (!featureEnabled) return Ok(Deny("AI Icebreakers are a Pro feature. Please upgrade your subscription to unlock them."));
                break;

            case ValidationAction.UseEmailTemplates:
                if (!featureEnabled) return Ok(Deny("Email Templates are a Pro feature. Please upgrade your subscription to unlock them."));
                break;

            case ValidationAction.UseBlocklist:
                if (!featureEnabled) return Ok(Deny("The Contact Blocklist is a Pro feature. Please upgrade your subscription to unlock it."));
                break;
        }

        return Ok(Approve());
    }

    private static int LimitFor(string action, int maxLeads, int maxContacts, int maxEmails) => action switch
    {
        ValidationAction.ExtractLeads => maxLeads > 0 ? maxLeads : -1,
        ValidationAction.ViewContacts => maxContacts,
        ValidationAction.SendEmails => maxEmails,
        _ => -1
    };
}