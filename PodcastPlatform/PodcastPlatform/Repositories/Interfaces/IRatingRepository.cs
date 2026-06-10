using PodcastPlatform.Models.Entities;

namespace PodcastPlatform.Repositories.Interfaces;

public interface IRatingRepository
{
    IQueryable<PodcastRating> Query();
    Task AddAsync(PodcastRating rating);
    Task UpdateAsync(PodcastRating rating);
    Task<bool> DeleteByIdAsync(int id);
}