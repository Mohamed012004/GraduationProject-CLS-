using Microsoft.EntityFrameworkCore;
using PodcastPlatform.Data;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Repositories.Interfaces;

namespace PodcastPlatform.Repositories.Implementations;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Subscription> Query()
    {
        return _context.Subscriptions;
    }

    public async Task<List<Subscription>> GetUserSubscriptionsAsync(string userId)
    {
        return await _context.Subscriptions.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
    }

    public async Task<List<Subscription>> GetPodcastSubscribersAsync(int podcastId)
    {
        return await _context.Subscriptions.Where(s => s.PodcastId == podcastId && s.IsActive).ToListAsync();
    }

    public async Task<Subscription?> GetActiveSubscriptionAsync(string userId, int podcastId)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.PodcastId == podcastId && s.IsActive);
    }

    public async Task<Subscription?> GetSubscriptionAsync(string userId, int podcastId)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.PodcastId == podcastId);
    }

    public async Task AddAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsUserSubscribedAsync(string userId, int podcastId)
    {
        return await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.PodcastId == podcastId && s.IsActive);
    }

    public async Task<int> GetSubscriberCountAsync(int podcastId)
    {
        return await _context.Subscriptions
            .Where(s => s.PodcastId == podcastId && s.IsActive)
            .CountAsync();
    }
}

