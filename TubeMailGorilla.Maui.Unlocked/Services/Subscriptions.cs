using TubeMailGorilla.Maui.Unlocked.Models;

namespace TubeMailGorilla.Maui.Unlocked.Services;

/// <summary>
/// Local subscription catalog. Mirrors the plans configured in the API's
/// appsettings.json ("SubscriptionPlans") — currently just the Pro plan.
/// The authoritative price is always fetched from GET /api/payments/pricing.
/// </summary>
public static class Subscriptions
{
    /// <summary>
    /// The unlocked edition has exactly one "plan" - everything. The catalog is
    /// kept so the settings pages can show what the edition includes.
    /// </summary>
    public static IReadOnlyList<SubscriptionPackage> Catalog { get; } = new List<SubscriptionPackage>
    {
        new()
        {
            Id = "unlocked",
            Name = "Unlocked",
            Price = "Included",
            Description = "Every feature is included: unlimited lead extraction, bulk CSV extraction, AI icebreakers, " +
                          "email templates, the blocklist and unlimited campaigns. No account, no subscription, no limits."
        }
    };

    /// <summary>Always the unlocked package - it cannot be downgraded or cancelled.</summary>
    public static SubscriptionPackage Current => Catalog[0];

    public static string CurrentId => Current.Id;

    /// <summary>No-op: a single-edition app has nothing to opt into.</summary>
    public static void OptIn(string id) { }

    /// <summary>No-op: the unlocked edition can never be locked out.</summary>
    public static void Cancel() { }
}