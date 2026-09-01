using TubeMailGorilla.Application.Interfaces;
using TubeMailGorilla.Domain.Constants;
using TubeMailGorilla.Domain.Interfaces;

namespace TubeMailGorilla.Application.Services;

/// <summary>
/// Application-layer use-case for the free-plan extraction quota. The server
/// (not the client) decides whether a non-paying user may run another
/// extraction - clients can't self-grant more runs by editing the app.
/// Paying users are never governed by this quota.
/// </summary>
public class ExtractionUsageService : IExtractionUsageService
{
    private readonly IUserRepository _userRepository;

    public ExtractionUsageService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> TryConsumeFreeExtractionAsync(string userId)
    {
        // Calendar month key, e.g. "2026-09". The free quota resets once per
        // calendar month, regardless of when within the month it was used.
        var monthKey = DateTime.UtcNow.ToString("yyyy-MM");

        // A claim for the current month means the free run is already used.
        if (await _userRepository.HasClaimAsync(userId, FreeExtractionClaim.Type, monthKey))
        {
            return false;
        }

        // Record this month's free extraction and approve it.
        await _userRepository.AddClaimAsync(userId, FreeExtractionClaim.Type, monthKey);
        return true;
    }
}