using Microsoft.EntityFrameworkCore;
using PodcastPlatform.Data;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Repositories.Interfaces;

namespace PodcastPlatform.Repositories.Implementations;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _context;

    public PlaylistRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Playlist> Query()
    {
        return _context.Playlists;
    }
    public IQueryable<PlaylistItem> QueryItems()
    {
        return _context.PlaylistItems;
    }


    public async Task<Playlist?> GetByIdAsync(int id)
    {
        return await _context.Playlists.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Playlist>> GetByOwnerAsync(string ownerId)
    {
        return await _context.Playlists.Where(p => p.OwnerId == ownerId).ToListAsync();
    }

    public async Task AddAsync(Playlist playlist)
    {
        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Playlist playlist)
    {
        _context.Playlists.Update(playlist);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteByIdAsync(int id)
    {
        var playlist = await _context.Playlists.FindAsync(id);
        if (playlist == null)
            return false;

        _context.Playlists.Remove(playlist);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddEpisodeToPlaylistAsync(int playlistId, int episodeId)
    {
        var playlist = await _context.Playlists.FindAsync(playlistId);
        var episode = await _context.Episodes.FindAsync(episodeId);

        if (playlist == null || episode == null)
            return false;

        var existingItem = await _context.PlaylistItems
            .FirstOrDefaultAsync(pi => pi.PlaylistId == playlistId && pi.EpisodeId == episodeId);

        if (existingItem != null)
            return false;

        var playlistItem = new PlaylistItem
        {
            PlaylistId = playlistId,
            EpisodeId = episodeId
        };

        _context.PlaylistItems.Add(playlistItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveEpisodeFromPlaylistAsync(int playlistId, int episodeId)
    {
        var playlistItem = await _context.PlaylistItems
            .FirstOrDefaultAsync(pi => pi.PlaylistId == playlistId && pi.EpisodeId == episodeId);

        if (playlistItem == null)
            return false;

        _context.PlaylistItems.Remove(playlistItem);
        await _context.SaveChangesAsync();
        return true;
    }
}

