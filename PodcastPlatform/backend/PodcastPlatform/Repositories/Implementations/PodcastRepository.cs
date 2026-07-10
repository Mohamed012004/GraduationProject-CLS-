using Microsoft.EntityFrameworkCore;
using PodcastPlatform.Data;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Repositories.Interfaces;

namespace PodcastPlatform.Repositories.Implementations;

public class PodcastRepository : IPodcastRepository
{
    private readonly AppDbContext _context;

    public PodcastRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Podcast> Query()
    {
        return _context.Podcasts;
    }
    
    public async Task<Podcast?> GetByIdAsync(int id)
    {
        return await _context.Podcasts.FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task AddAsync(Podcast podcast)
    {
        _context.Podcasts.Add(podcast);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Podcast podcast)
    {
        _context.Podcasts.Update(podcast);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var podcast = await _context.Podcasts.FindAsync(id);
        if (podcast == null)
            return false;

        _context.Podcasts.Remove(podcast);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetSubscriberCountAsync(int podcastId)
    {
        return await _context.Subscriptions
            .Where(s => s.PodcastId == podcastId && s.IsActive)
            .CountAsync();
    }

    public async Task IncrementSubscriberCountAsync(int podcastId)
    {
        var podcast = await _context.Podcasts.FindAsync(podcastId);
        if (podcast == null)
            return;

        podcast.SubscriberCount++;
        await _context.SaveChangesAsync();
    }

    public async Task DecrementSubscriberCountAsync(int podcastId)
    {
        var podcast = await _context.Podcasts.FindAsync(podcastId);
        if (podcast == null)
            return;

        if (podcast.SubscriberCount > 0)
            podcast.SubscriberCount--;

        await _context.SaveChangesAsync();
    }
}
