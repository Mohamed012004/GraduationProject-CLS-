using PodcastPlatform.DTOs.Playlist;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Interfaces;

public interface IPlaylistService
{
    Task<ServiceResult<IEnumerable<PlaylistResponseDto>>> GetAllPlaylistsAsync();
    Task<ServiceResult<PlaylistResponseDto>> GetPlaylistByIdAsync(int id);
    Task<ServiceResult<IEnumerable<PlaylistResponseDto>>> GetPlaylistsByOwnerAsync(string ownerId);
    Task<ServiceResult<PlaylistResponseDto>> CreatePlaylistAsync(CreatePlaylistDto dto, string? userId);
    Task<ServiceResult> UpdatePlaylistAsync(int id, UpdatePlaylistDto dto, string? userId);
    Task<ServiceResult> DeletePlaylistAsync(int id, string? userId);
    Task<ServiceResult> AddEpisodeToPlaylistAsync(int playlistId, int episodeId, string? userId);
    Task<ServiceResult> RemoveEpisodeFromPlaylistAsync(int playlistId, int episodeId, string? userId);
    Task CreateDefaultPlaylistsAsync(string userId);
}