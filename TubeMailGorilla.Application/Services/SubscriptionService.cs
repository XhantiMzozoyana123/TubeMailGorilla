using Microsoft.Extensions.Options;
using TubeMailGorilla.Application.DTOs;
using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain;
using TubeMailGorilla.Domain.Constants;
using TubeMailGorilla.Domain.Entities;
using TubeMailGorilla.Domain.Interfaces;

namespace TubeMailGorilla.Application.Services;

/// <summary>
/// Application-layer use-case for RECURRING subscription operations.
/// - CreatePaymentAsync starts a PayPal subscription checkout (approval URL).
/// - CapturePaymentAsync verifies approval at PayPal and activates locally.
/// - CancelSubscriptionAsync cancels AT PAYPAL first so future monthly
///   charges stop, then removes the entitlement claim.
/// Prices come from configuration; the client never dictates an amount.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
        private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly PricingSettings _pricing;
    private readonly SubscriptionPlansOptions _plans;

    public SubscriptionService(
        IPaymentGateway paymentGateway,
        IUserRepository userRepository,
        ITokenService tokenService,
        ISubscriptionRepository subscriptionRepository,
        IOptions<PricingSettings> pricingOptions,
        IOptions<SubscriptionPlansOptions> plansOptions)
    {
        _paymentGateway = paymentGateway;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _subscriptionRepository = subscriptionRepository;
        _pricing = pricingOptions.Value;
        _plans = plansOptions.Value;
    }

    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
    {
        // SECURITY: pricing is server-side configuration only.
        var checkout = await _paymentGateway.StartSubscriptionAsync(
            request.ReturnUrl ?? string.Empty,
            request.CancelUrl ?? string.Empty);

        if (!checkout.Success || string.IsNullOrEmpty(checkout.PayPalSubscriptionId))
        {
            return new PaymentResponse { Success = false, Message = checkout.Error ?? "Could not start PayPal subscription checkout." };
        }

        // Track it. PayPalOrderId column stores the PAYPAL SUBSCRIPTION id -
        // the durable reference needed to cancel future charges later.
        await _subscriptionRepository.AddAsync(new Subscription
        {
            UserId = request.UserId!,
            PayPalOrderId = checkout.PayPalSubscriptionId,
            Amount = _pricing.Amount,
            Currency = _pricing.Currency,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        });

        return new PaymentResponse
        {
            Success = true,
            OrderId = checkout.PayPalSubscriptionId,
            ApprovalUrl = checkout.ApprovalUrl,
            Message = "Subscription checkout created successfully."
        };
    }
    /// <summary>
    /// Called when the buyer returns from PayPal approval. Verifies the
    /// subscription state AT PAYPAL before granting entitlements.
    /// </summary>
    public async Task<CapturePaymentResponse> CapturePaymentAsync(CapturePaymentRequest request, string userId)
    {
        var subscription = await _subscriptionRepository.GetByOrderIdAsync(request.OrderId);
        if (subscription is null || subscription.UserId != userId)
        {
            // The redirect param PayPal sends back can be inconsistent across
            // flows (subscription_id vs token vs none). Rather than fail, fall
            // back to the caller's own most-recent Pending subscription.
            subscription = await _subscriptionRepository.GetLatestPendingForUserAsync(userId);
        }

        if (subscription is null)
        {
            return new CapturePaymentResponse { Success = false, IsSubscribed = false, Message = "No pending subscription found to activate. If you paid, try again or contact support." };
        }

        if (string.Equals(subscription.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return new CapturePaymentResponse { Success = true, IsSubscribed = true, Token = await GenerateFreshTokenAsync(userId), Message = "Subscription already active." };
        }

        if (!string.Equals(subscription.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return new CapturePaymentResponse { Success = false, IsSubscribed = false, Message = $"Subscription is not activatable (status: {subscription.Status})." };
        }

        // Verify the approval at PayPal using the matched subscription's id.
        var remote = await _paymentGateway.GetRemoteStatusAsync(subscription.PayPalOrderId);
        if (!remote.Success || string.IsNullOrEmpty(remote.Status))
        {
            return new CapturePaymentResponse { Success = false, IsSubscribed = false, Message = $"Could not verify subscription at PayPal. {remote.Error}" };
        }

        if (!remote.Status.Equals("ACTIVE", StringComparison.OrdinalIgnoreCase)
            && !remote.Status.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
        {
            return new CapturePaymentResponse { Success = false, IsSubscribed = false, Message = $"Subscription is not approved yet (PayPal status: {remote.Status})." };
        }

        // Sanity: PayPal's plan charge must match our configured price.
        if (remote.LastPaymentAmount != 0 && remote.LastPaymentAmount != subscription.Amount)
        {
            subscription.Status = "AmountMismatch";
            await _subscriptionRepository.UpdateAsync(subscription);
            return new CapturePaymentResponse { Success = false, IsSubscribed = false, Message = $"Charged amount mismatch (expected {subscription.Amount} {subscription.Currency}, got {remote.LastPaymentAmount} {remote.Currency})." };
        }

        subscription.Status = "Active";
        subscription.ActivatedAtUtc = DateTime.UtcNow;
        await _subscriptionRepository.UpdateAsync(subscription);

        if (!await _userRepository.HasClaimAsync(userId, SubscriptionClaim.Type, SubscriptionClaim.Value))
        {
            await _userRepository.AddClaimAsync(userId, SubscriptionClaim.Type, SubscriptionClaim.Value);
        }

        return new CapturePaymentResponse
        {
            Success = true,
            IsSubscribed = true,
            Token = await GenerateFreshTokenAsync(userId),
            Message = "Subscription activated - recurring monthly billing is now enabled."
        };
    }
        public async Task<bool> IsSubscribedAsync(string userId)
    {
        return await _userRepository.HasClaimAsync(userId, SubscriptionClaim.Type, SubscriptionClaim.Value);
    }

    /// <summary>
    /// Builds the customer-facing subscription state from the plan catalog
    /// plus the latest active subscription record.
    /// </summary>
    public async Task<SubscriptionStatusResponse> GetStatusAsync(string userId)
    {
        var isSubscribed = await IsSubscribedAsync(userId);
        var plan = _plans.DefaultPlan;

        if (!isSubscribed || plan is null)
        {
            return new SubscriptionStatusResponse
            {
                IsSubscribed = false,
                PlanId = "free",
                PlanName = "Free"
            };
        }

        var active = await _subscriptionRepository.GetActiveForUserAsync(userId);

        // PayPal bills monthly from activation; show that as the next charge.
        DateTime? nextBilling = active?.ActivatedAtUtc is not null
            ? active.ActivatedAtUtc.Value.AddMonths(1)
            : null;

        return new SubscriptionStatusResponse
        {
            IsSubscribed = true,
            PlanId = plan.Id,
            PlanName = plan.Name,
            Tagline = plan.Tagline,
            Price = plan.MonthlyPrice,
            Currency = plan.Currency,
            NextBillingDate = nextBilling
        };
    }

    public async Task<CancelSubscriptionResponse> CancelSubscriptionAsync(string userId)
    {
        var wasSubscribed = await _userRepository.HasClaimAsync(userId, SubscriptionClaim.Type, SubscriptionClaim.Value);

        var active = await _subscriptionRepository.GetActiveForUserAsync(userId);

        // CRITICAL: cancel the recurring billing AT PAYPAL so future monthly
        // charges stop. If PayPal refuses, abort and let the user retry - we
        // must never strip the local claim while PayPal would keep billing.
        if (active is not null)
        {
            var remoteCancelled = await _paymentGateway.CancelRemoteSubscriptionAsync(
                active.PayPalOrderId,
                "Customer requested cancellation");

            if (!remoteCancelled)
            {
                return new CancelSubscriptionResponse
                {
                    Success = false,
                    IsSubscribed = true,
                    Message = "Could not cancel the billing agreement at PayPal right now. Please try again shortly - you have NOT been charged further."
                };
            }

            active.Status = "Cancelled";
            active.CancelledAtUtc = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(active);
        }

        if (wasSubscribed)
        {
            await _userRepository.RemoveClaimAsync(userId, SubscriptionClaim.Type, SubscriptionClaim.Value);
        }

        return new CancelSubscriptionResponse
        {
            Success = true,
            IsSubscribed = false,
            Token = await GenerateFreshTokenAsync(userId),
            Message = active is not null
                ? "Subscription cancelled at PayPal - future monthly charges have been stopped."
                : wasSubscribed ? "Subscription cancelled." : "No active subscription found."
        };
    }

    private async Task<string?> GenerateFreshTokenAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userRepository.GetRolesAsync(userId);
        var claims = await _userRepository.GetClaimsAsync(userId);
        return _tokenService.GenerateToken(user, roles, claims);
    }
}