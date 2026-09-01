namespace TubeMailGorilla.Application.Interfaces;

/// <summary>
/// Use-case contract for the free-plan extraction quota. Lets the gatekeeper
/// enforce exactly one free extraction per calendar month server-side.
/// Paying users are never governed by this quota.
/// </summary>
public interface IExtractionUsageService
{
    /// <summary>
    /// Tries to consume one free extraction for the user. Returns true (and
    /// records the run) when they still have quota this calendar month; returns
    /// false (without recording) when the monthly free quota is already used up.
    /// </summary>
    Task<bool> TryConsumeFreeExtractionAsync(string userId);
}