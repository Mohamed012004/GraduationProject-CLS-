using PodcastPlatform.Models.Entities;

namespace PodcastPlatform.Repositories.Interfaces;

public interface ISubscriptionRepository
{
    IQueryable<Subscription> Query();
    Task<Subscription?> GetSubscriptionAsync(string userId, int podcastId);
    Task AddAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task<bool> IsUserSubscribedAsync(string userId, int podcastId);
    Task<int> GetSubscriberCountAsync(int podcastId);
}

