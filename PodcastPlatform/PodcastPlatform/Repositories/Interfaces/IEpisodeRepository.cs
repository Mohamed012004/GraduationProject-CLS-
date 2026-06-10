using PodcastPlatform.Models.Entities;

namespace PodcastPlatform.Repositories.Interfaces;

public interface IEpisodeRepository
{
    IQueryable<Episode> Query();
    Task<Episode?> GetByIdAsync(int id);
    Task<List<Episode>> GetByPodcastAsync(int podcastId);
    Task AddAsync(Episode episode);
    Task UpdateAsync(Episode episode);
    Task<bool> DeleteByIdAsync(int id);
    Task IncrementViewCountAsync(int episodeId);
}

