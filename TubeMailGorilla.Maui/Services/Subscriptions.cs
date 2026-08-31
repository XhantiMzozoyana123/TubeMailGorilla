using TubeMailGorilla.Maui.Models;

namespace TubeMailGorilla.Maui.Services;

/// <summary>
/// Local subscription catalog. Mirrors the plans configured in the API's
/// appsettings.json ("SubscriptionPlans") — currently just the Pro plan.
/// The authoritative price is always fetched from GET /api/payments/pricing.
/// </summary>
public static class Subscriptions
{
    private const string ActiveKey = "ActiveSubscription";

    public static IReadOnlyList<SubscriptionPackage> Catalog { get; } = new List<SubscriptionPackage>
    {
        new() { Id = "free", Name = "Free", Price = "$0", Description = "Try TubeMail Gorilla with up to 100 leads during your 14-day trial." },
        new() { Id = "pro", Name = "Pro", Price = "$9.99/mo", Description = "5,000 verified creator leads every month, unlimited outreach templates, AI-written pitch emails, and access on web + desktop. Cancel anytime." }
    };

    public static string? CurrentId
    {
        get
        {
            var id = Preferences.Default.Get(ActiveKey, string.Empty);
            return string.IsNullOrWhiteSpace(id) ? null : id;
        }
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                Preferences.Default.Remove(ActiveKey);
            else
                Preferences.Default.Set(ActiveKey, value);
        }
    }

    public static SubscriptionPackage? Current => Catalog.FirstOrDefault(p => p.Id == CurrentId);

    public static void OptIn(string id)
    {
        if (Catalog.Any(p => p.Id == id))
            CurrentId = id;
    }

    public static void Cancel() => CurrentId = null;
}