using Microsoft.EntityFrameworkCore;
using TubeMailGorilla.Domain.Entities;
using TubeMailGorilla.Domain.Interfaces;
using TubeMailGorilla.Infrastructure.Data;

namespace TubeMailGorilla.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISubscriptionRepository"/>.
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ApplicationDbContext _db;

    public SubscriptionRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Subscription subscription)
    {
        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync();
    }

    public Task<Subscription?> GetByOrderIdAsync(string payPalOrderId)
    {
        return _db.Subscriptions
            .FirstOrDefaultAsync(s => s.PayPalOrderId == payPalOrderId);
    }

    public Task<Subscription?> GetActiveForUserAsync(string userId)
    {
        return _db.Subscriptions
            .Where(s => s.UserId == userId && s.Status == "Active")
            .OrderByDescending(s => s.ActivatedAtUtc)
            .FirstOrDefaultAsync();
    }

    public Task<Subscription?> GetLatestPendingForUserAsync(string userId)
    {
        return _db.Subscriptions
            .Where(s => s.UserId == userId && s.Status == "Pending")
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        _db.Subscriptions.Update(subscription);
        await _db.SaveChangesAsync();
    }
}