using PodcastPlatform.Models.Entities;

namespace PodcastPlatform.Repositories.Interfaces;

public interface IPlaylistRepository
{
    IQueryable<Playlist> Query();
    Task<Playlist?> GetByIdAsync(int id);
    Task AddAsync(Playlist playlist);
    Task UpdateAsync(Playlist playlist);
    Task<bool> DeleteByIdAsync(int id);
    Task<bool> AddEpisodeToPlaylistAsync(int playlistId, int episodeId);
    Task<bool> RemoveEpisodeFromPlaylistAsync(int playlistId, int episodeId);
}

