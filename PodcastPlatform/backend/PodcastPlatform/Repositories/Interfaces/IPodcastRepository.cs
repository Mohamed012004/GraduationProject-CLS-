using PodcastPlatform.Models.Entities;

namespace PodcastPlatform.Repositories.Interfaces;

public interface IPodcastRepository
{
    IQueryable<Podcast> Query();
    Task<Podcast?> GetByIdAsync(int id);
    Task AddAsync(Podcast podcast);
    Task UpdateAsync(Podcast podcast);
    Task<bool> DeleteByIdAsync(int id);
    Task<int> GetSubscriberCountAsync(int podcastId);
    Task IncrementSubscriberCountAsync(int podcastId);
    Task DecrementSubscriberCountAsync(int podcastId);
}

