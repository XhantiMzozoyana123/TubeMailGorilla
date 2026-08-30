namespace TubeMailGorilla.Application.DTOs;

/// <summary>
/// What the current user's plan allows. The server is the single source of
/// truth - the desktop app reads this on every page load and enforces it.
/// A value of -1 means "unlimited".
/// </summary>
public record EntitlementsDto(
    bool IsSubscribed,
    string PlanId,
    string PlanName,
    int MaxLeadsPerExtraction,
    int MaxContactsVisible,
    int MaxEmailsPerCampaign,
    bool IcebreakerEnabled,
    bool EmailTemplatesEnabled,
    bool BlocklistEnabled);