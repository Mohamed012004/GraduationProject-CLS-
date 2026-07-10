using Microsoft.EntityFrameworkCore;
using PodcastPlatform.Data;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Repositories.Interfaces;

namespace PodcastPlatform.Repositories.Implementations;

public class EpisodeRepository : IEpisodeRepository
{
    private readonly AppDbContext _context;

    public EpisodeRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Episode> Query()
    {
        return _context.Episodes;
    }

    public async Task<List<Episode>> GetPublicAsync()
    {
        return await _context.Episodes
            .Where(e => e.Privacy == PrivacyType.Public)
            .OrderByDescending(e => e.PublishedAt)
            .ToListAsync();
    }

    public async Task<Episode?> GetByIdAsync(int id)
    {
        return await _context.Episodes.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Episode>> GetByPodcastAsync(int podcastId)
    {
        return await _context.Episodes
            .Where(e => e.PodcastId == podcastId)
            .OrderByDescending(e => e.PublishedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Episode episode)
    {
        _context.Episodes.Add(episode);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Episode episode)
    {
        _context.Episodes.Update(episode);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var episode = await _context.Episodes.FindAsync(id);
        if (episode == null)
            return false;

        _context.Episodes.Remove(episode);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task IncrementViewCountAsync(int episodeId)
    {
        var episode = await _context.Episodes.FindAsync(episodeId);
        if (episode == null)
            return;

        episode.ViewCount++;
        await _context.SaveChangesAsync();
    }
}

