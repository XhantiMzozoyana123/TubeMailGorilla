namespace TubeMailGorilla.Domain.Constants;

/// <summary>
/// Well-known claim types and values used for subscription-based authorization.
/// These are referenced by the Application and Presentation layers when
/// issuing tokens and evaluating authorization policies.
/// </summary>
public static class SubscriptionClaim
{
    public const string Type = "subscription";
    public const string Value = "active";
}
