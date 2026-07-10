using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodcastPlatform.DTOs.Playlist;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Repositories.Interfaces;
using PodcastPlatform.Services.Interfaces;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Implementations;

public class PlaylistService : IPlaylistService
{
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly ILogger<PlaylistService> _logger;

    public PlaylistService(
        IPlaylistRepository playlistRepository,
        IEpisodeRepository episodeRepository,
        ILogger<PlaylistService> logger)
    {
        _playlistRepository = playlistRepository;
        _episodeRepository = episodeRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IEnumerable<PlaylistResponseDto>>> GetAllPlaylistsAsync()
    {
        var response = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.Privacy == PrivacyType.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Select(PlaylistProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<PlaylistResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<PlaylistResponseDto>> GetPlaylistByIdAsync(int id)
    {
        var playlist = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(PlaylistProjection)
            .FirstOrDefaultAsync();

        if (playlist == null)
            return ServiceResult<PlaylistResponseDto>.NotFound("Playlist not found");

        return ServiceResult<PlaylistResponseDto>.Ok(playlist);
    }

    public async Task<ServiceResult<IEnumerable<PlaylistResponseDto>>> GetPlaylistsByOwnerAsync(string ownerId)
    {
        var response = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(PlaylistProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<PlaylistResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<PlaylistResponseDto>> CreatePlaylistAsync(CreatePlaylistDto dto, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<PlaylistResponseDto>.Unauthorized();

        var playlist = new Playlist
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = PlaylistType.Normal,
            OwnerId = userId,
            Privacy = dto.Privacy,
            CreatedAt = DateTime.UtcNow,
            IsSystemPlaylist = false
        };

        await _playlistRepository.AddAsync(playlist);
        var response = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == playlist.Id)
            .Select(PlaylistProjection)
            .FirstOrDefaultAsync() ?? new PlaylistResponseDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            Type = playlist.Type,
            OwnerId = playlist.OwnerId,
            OwnerName = string.Empty,
            Privacy = playlist.Privacy,
            IsSystemPlaylist = playlist.IsSystemPlaylist,
            ItemCount = 0,
            CreatedAt = playlist.CreatedAt,
            UpdatedAt = playlist.UpdatedAt
        };

        return ServiceResult<PlaylistResponseDto>.Created(response, "GetPlaylist", new { id = playlist.Id });
    }

    public async Task<ServiceResult> UpdatePlaylistAsync(int id, UpdatePlaylistDto dto, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var playlistInfo = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.OwnerId, p.IsSystemPlaylist })
            .FirstOrDefaultAsync();

        if (playlistInfo == null)
            return ServiceResult.NotFound("Playlist not found");

        if (playlistInfo.OwnerId != userId)
            return ServiceResult.Forbidden("You can only update your own playlists");

        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
            return ServiceResult.NotFound("Playlist not found");

        playlist.Name = dto.Name ?? playlist.Name;
        playlist.Description = dto.Description ?? playlist.Description;

        playlist.Privacy = dto.Privacy ?? playlist.Privacy;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _playlistRepository.UpdateAsync(playlist);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeletePlaylistAsync(int id, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var playlistInfo = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.OwnerId, p.IsSystemPlaylist })
            .FirstOrDefaultAsync();

        if (playlistInfo == null)
            return ServiceResult.NotFound("Playlist not found");

        if (playlistInfo.OwnerId != userId)
            return ServiceResult.Forbidden("You can only delete your own playlists");

        if (playlistInfo.IsSystemPlaylist)
            return ServiceResult.BadRequest("System playlists (Liked and Watch Later) cannot be deleted");

        var playlist = await _playlistRepository.GetByIdAsync(id);
        if (playlist == null)
            return ServiceResult.NotFound("Playlist not found");

        await _playlistRepository.DeleteByIdAsync(id);
        return ServiceResult.NoContent();
    }

    public async Task CreateDefaultPlaylistsAsync(string userId)
    {
        try
        {
            _logger.LogInformation($"Starting to create default playlists for user: {userId}");

            var likedPlaylist = new Playlist
            {
                Name = "Liked",
                Type = PlaylistType.Liked,
                OwnerId = userId,
                IsSystemPlaylist = true,
                Privacy = PrivacyType.Private,
                CreatedAt = DateTime.UtcNow
            };

            var watchLaterPlaylist = new Playlist
            {
                Name = "Watch Later",
                Type = PlaylistType.WatchLater,
                OwnerId = userId,
                IsSystemPlaylist = true,
                Privacy = PrivacyType.Private,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation($"Adding Liked playlist for user {userId}");
            await _playlistRepository.AddAsync(likedPlaylist);

            _logger.LogInformation($"Adding Watch Later playlist for user {userId}");
            await _playlistRepository.AddAsync(watchLaterPlaylist);

            _logger.LogInformation($"Successfully created default playlists for user {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating default playlists for user {userId}: {ex.Message} | {ex.InnerException?.Message}");
            throw;
        }
    }

    public async Task<ServiceResult> AddEpisodeToPlaylistAsync(int playlistId, int episodeId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
            return ServiceResult.NotFound("Playlist not found");

        if (playlist.OwnerId != userId)
            return ServiceResult.Forbidden("You can only add episodes to your own playlists");

        var episode = await _episodeRepository.GetByIdAsync(episodeId);
        if (episode == null)
            return ServiceResult.NotFound("Episode not found");

        var success = await _playlistRepository.AddEpisodeToPlaylistAsync(playlistId, episodeId);
        if (!success)
            return ServiceResult.BadRequest("Episode is already in this playlist");

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> RemoveEpisodeFromPlaylistAsync(int playlistId, int episodeId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var playlist = await _playlistRepository.GetByIdAsync(playlistId);
        if (playlist == null)
            return ServiceResult.NotFound("Playlist not found");

        if (playlist.OwnerId != userId)
            return ServiceResult.Forbidden("You can only remove episodes from your own playlists");

        var success = await _playlistRepository.RemoveEpisodeFromPlaylistAsync(playlistId, episodeId);
        if (!success)
            return ServiceResult.NotFound("Episode not found in this playlist");

        return ServiceResult.NoContent();
    }

    private static readonly Expression<Func<Playlist, PlaylistResponseDto>> PlaylistProjection = playlist => new PlaylistResponseDto
    {
        Id = playlist.Id,
        Name = playlist.Name,
        Description = playlist.Description,
        Type = playlist.Type,
        OwnerId = playlist.OwnerId,
        OwnerName = playlist.Owner.UserName ?? string.Empty,
        Privacy = playlist.Privacy,
        IsSystemPlaylist = playlist.IsSystemPlaylist,
        ItemCount = playlist.Items.Count,
        CreatedAt = playlist.CreatedAt,
        UpdatedAt = playlist.UpdatedAt
    };
}