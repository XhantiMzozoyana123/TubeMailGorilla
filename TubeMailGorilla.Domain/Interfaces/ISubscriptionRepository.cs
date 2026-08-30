using TubeMailGorilla.Domain.Entities;

namespace TubeMailGorilla.Domain.Interfaces;

/// <summary>
/// Persistence contract for subscription lifecycle records.
/// </summary>
public interface ISubscriptionRepository
{
    Task AddAsync(Subscription subscription);
    Task<Subscription?> GetByOrderIdAsync(string payPalOrderId);
    Task<Subscription?> GetActiveForUserAsync(string userId);
    Task<Subscription?> GetLatestPendingForUserAsync(string userId);
    Task UpdateAsync(Subscription subscription);
}