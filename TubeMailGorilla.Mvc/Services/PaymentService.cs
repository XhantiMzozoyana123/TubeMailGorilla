using System.Threading.Tasks;

namespace TubeMailGorilla.Mvc.Services
{
    /// <summary>
    /// UNLOCKED EDITION: this app has no subscriptions, no PayPal checkout and
    /// no server. The class keeps the same public surface as the subscription
    /// edition so the pages do not need to change, but every answer is a local
    /// "unlimited / everything enabled" one - nothing is ever locked out.
    /// </summary>
    public class PaymentService
    {
        /// <summary>The unlocked edition is itself the "everything included" plan.</summary>
        public const string UnlockedPlanId = "unlocked";

        /// <summary>Display name of the edition (shown on the Unlocked tab / Settings).</summary>
        public const string UnlockedPlanName = "Unlocked";

        private static readonly EntitlementInfo UnlimitedEntitlements = new()
        {
            IsSubscribed = true,
            PlanId = UnlockedPlanId,
            PlanName = UnlockedPlanName,
            MaxLeadsPerExtraction = -1,
            MaxContactsVisible = -1,
            MaxEmailsPerCampaign = -1,
            IcebreakerEnabled = true,
            EmailTemplatesEnabled = true,
            BlocklistEnabled = true
        };

        /// <summary>Always returns the unlimited entitlement set (no HTTP call).</summary>
        public Task<EntitlementInfo> GetEntitlementsAsync() => Task.FromResult(UnlimitedEntitlements);

        /// <summary>Nothing is cached from a server, so this is a no-op.</summary>
        public void InvalidateEntitlementsCache() { }

        /// <summary>Blocklist page is fully available in the unlocked edition.</summary>
        public Task<bool> IsBlocklistAllowedAsync() => Task.FromResult(true);

        /// <summary>Always "subscribed" - i.e. every feature is available.</summary>
        public Task<bool> IsSubscribedAsync() => Task.FromResult(true);

        /// <summary>No price list: the unlocked edition is not sold in-app.</summary>
        public Task<PricingInfo?> GetPricingAsync() => Task.FromResult<PricingInfo?>(null);

        /// <summary>Reports the unlocked edition instead of a paid plan.</summary>
        public Task<SubscriptionStatusInfo?> GetSubscriptionStatusAsync() => Task.FromResult<SubscriptionStatusInfo?>(
            new SubscriptionStatusInfo
            {
                IsSubscribed = true,
                PlanId = UnlockedPlanId,
                PlanName = UnlockedPlanName,
                Tagline = "Every feature included - no account, no subscription, no limits.",
                Price = 0m,
                Currency = "USD",
                NextBillingDate = null
            });

        /// <summary>
        /// Kept for call-site parity. In the unlocked edition there is nothing
        /// to upgrade to, so the website is not opened.
        /// </summary>
        public string GetUpgradeWebsiteUrl() => string.Empty;

        /// <summary>Checkout is not part of the unlocked edition.</summary>
        public Task<(string? OrderId, string? ApprovalUrl, string? Error)> CreateOrderAsync(string returnUrl, string cancelUrl)
            => Task.FromResult<(string?, string?, string?)>((null, null, "This edition is fully unlocked - there is nothing to buy."));

        /// <summary>Checkout is not part of the unlocked edition.</summary>
        public Task<(bool Success, string? Message)> CaptureOrderAsync(string orderId)
            => Task.FromResult<(bool, string?)>((false, "This edition is fully unlocked - there is nothing to purchase."));

        /// <summary>There is no subscription to cancel.</summary>
        public Task<(bool Success, string? Message)> CancelSubscriptionAsync()
            => Task.FromResult<(bool, string?)>((false, "This edition has no subscription to cancel."));
    }

    /// <summary>Kept for parity with the subscription edition (unused here).</summary>
    public class PricingInfo
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    /// <summary>Kept for parity with the subscription edition.</summary>
    public class SubscriptionStatusInfo
    {
        public bool IsSubscribed { get; set; }
        public string PlanId { get; set; } = PaymentService.UnlockedPlanId;
        public string PlanName { get; set; } = PaymentService.UnlockedPlanName;
        public string? Tagline { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? NextBillingDate { get; set; }
    }

    /// <summary>
    /// Feature switches read by the pages. The unlocked defaults enable
    /// everything and mark every limit as unlimited (-1).
    /// </summary>
    public class EntitlementInfo
    {
        public bool IsSubscribed { get; set; } = true;
        public string PlanId { get; set; } = PaymentService.UnlockedPlanId;
        public string PlanName { get; set; } = PaymentService.UnlockedPlanName;

        /// <summary>Max leads per extraction run (-1 = no cap).</summary>
        public int MaxLeadsPerExtraction { get; set; } = -1;

        /// <summary>Max contacts shown on the contacts page (-1 = unlimited).</summary>
        public int MaxContactsVisible { get; set; } = -1;

        /// <summary>Max recipients per send campaign (-1 = unlimited).</summary>
        public int MaxEmailsPerCampaign { get; set; } = -1;

        public bool IcebreakerEnabled { get; set; } = true;
        public bool EmailTemplatesEnabled { get; set; } = true;
        public bool BlocklistEnabled { get; set; } = true;

        public bool IsUnlimited(int limit) => limit < 0;
    }
}
