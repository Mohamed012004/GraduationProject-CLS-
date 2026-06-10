using Microsoft.EntityFrameworkCore;
using PodcastPlatform.Data;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Repositories.Interfaces;

namespace PodcastPlatform.Repositories.Implementations;

public class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<PodcastRating> Query()
    {
        return _context.PodcastRatings;
    }
    

    public async Task AddAsync(PodcastRating rating)
    {
        _context.PodcastRatings.Add(rating);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PodcastRating rating)
    {
        _context.PodcastRatings.Update(rating);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var rating = await _context.PodcastRatings.FindAsync(id);
        if (rating == null)
            return false;

        _context.PodcastRatings.Remove(rating);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<double> GetAverageRatingAsync(int podcastId)
    {
        var ratings = await _context.PodcastRatings
            .Where(r => r.PodcastId == podcastId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (ratings.Count == 0)
            return 0;

        return ratings.Average();
    }

    public async Task<int> GetRatingCountAsync(int podcastId)
    {
        return await _context.PodcastRatings
            .Where(r => r.PodcastId == podcastId)
            .CountAsync();
    }
}
