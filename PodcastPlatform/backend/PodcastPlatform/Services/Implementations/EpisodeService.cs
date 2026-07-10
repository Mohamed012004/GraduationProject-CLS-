using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PodcastPlatform.DTOs.Episode;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Repositories.Interfaces;
using PodcastPlatform.Services.Interfaces;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Implementations;

public class EpisodeService : IEpisodeService
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IPodcastRepository _podcastRepository;
    private readonly IPlaylistRepository _playlistRepository;

    public EpisodeService(
        IEpisodeRepository episodeRepository,
        IPodcastRepository podcastRepository,
        ICloudinaryService cloudinaryService,
        IPlaylistRepository playlistRepository)
    {
        _episodeRepository = episodeRepository;
        _podcastRepository = podcastRepository;
        _cloudinaryService = cloudinaryService;
        _playlistRepository = playlistRepository;
    }

    public async Task<ServiceResult<IEnumerable<EpisodeResponseDto>>> GetAllEpisodesAsync()
    {
        var response = await _episodeRepository.Query()
            .AsNoTracking()
            .Where(e => e.Privacy == PrivacyType.Public)
            .OrderByDescending(e => e.PublishedAt)
            .Select(EpisodeProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<EpisodeResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<EpisodeResponseDto>> GetEpisodeByIdAsync(int id, string? requesterUserId)
    {
        var episode = await _episodeRepository.Query()
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Privacy,
                OwnerId = e.Podcast.OwnerId,
                Response = new EpisodeResponseDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    AudioUrl = e.AudioUrl,
                    ImageUrl = e.ImageUrl,
                    Duration = e.Duration,
                    PodcastId = e.PodcastId,
                    PodcastTitle = e.Podcast.Title,
                    Privacy = e.Privacy,
                    ViewCount = e.ViewCount,
                    PublishedAt = e.PublishedAt,
                    UpdatedAt = e.UpdatedAt
                }
            })
            .FirstOrDefaultAsync();

        if (episode == null)
            return ServiceResult<EpisodeResponseDto>.NotFound("Episode not found");

        if (episode.Privacy == PrivacyType.Private && episode.OwnerId != requesterUserId)
            return ServiceResult<EpisodeResponseDto>.Forbidden();

        await _episodeRepository.IncrementViewCountAsync(id);
        return ServiceResult<EpisodeResponseDto>.Ok(episode.Response);
    }

    public async Task<ServiceResult<IEnumerable<EpisodeResponseDto>>> GetEpisodesByPodcastAsync(int podcastId)
    {
        var podcastExists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!podcastExists)
            return ServiceResult<IEnumerable<EpisodeResponseDto>>.NotFound("Podcast not found");

        var response = await _episodeRepository.Query()
            .AsNoTracking()
            .Where(e => e.PodcastId == podcastId)
            .OrderByDescending(e => e.PublishedAt)
            .Select(EpisodeProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<EpisodeResponseDto>>.Ok(response);
    }
    public async Task<ServiceResult<IEnumerable<EpisodeResponseDto>>> GetEpisodesByPlaylistAsync(int playlistId)
    {
        var playlistExists = await _playlistRepository.Query().AsNoTracking().AnyAsync(p => p.Id== playlistId);
        if (!playlistExists)
            return ServiceResult<IEnumerable<EpisodeResponseDto>>.NotFound("Podcast not found");

        var response = await _playlistRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == playlistId)
            .SelectMany(p => p.Items.Select(item => item.Episode)) // 1. Flatten to Episodes
            .Select(EpisodeProjection)                            // 2. Project to DTO
            .ToListAsync();

        return ServiceResult<IEnumerable<EpisodeResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<EpisodeResponseDto>> CreateEpisodeAsync(CreateEpisodeDto dto, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<EpisodeResponseDto>.Unauthorized();

        var podcast = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == dto.PodcastId)
            .Select(p => new { p.Id, p.OwnerId, p.Title })
            .FirstOrDefaultAsync();

        if (podcast == null)
            return ServiceResult<EpisodeResponseDto>.NotFound("Podcast not found");

        if (podcast.OwnerId != userId)
            return ServiceResult<EpisodeResponseDto>.Forbidden("You can only add episodes to your own podcasts");

        var (audioUrl, duration) = await _cloudinaryService.UploadAudioAsync(dto.AudioFile);
        string? imageUrl = null;
        if (dto.ImageFile != null)
            imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);

        var episode = new Episode
        {
            Title = dto.Title,
            Description = dto.Description,
            AudioUrl = audioUrl,
            ImageUrl = imageUrl,
            Duration = duration,
            PodcastId = dto.PodcastId,
            Privacy = dto.Privacy,
            PublishedAt = DateTime.UtcNow
        };

        await _episodeRepository.AddAsync(episode);
        var response = await _episodeRepository.Query()
            .AsNoTracking()
            .Where(e => e.Id == episode.Id)
            .Select(EpisodeProjection)
            .FirstOrDefaultAsync() ?? new EpisodeResponseDto
        {
            Id = episode.Id,
            Title = episode.Title,
            Description = episode.Description,
            AudioUrl = episode.AudioUrl,
            ImageUrl = episode.ImageUrl,
            Duration = episode.Duration,
            PodcastId = episode.PodcastId,
            PodcastTitle = podcast.Title,
            Privacy = episode.Privacy,
            ViewCount = episode.ViewCount,
            PublishedAt = episode.PublishedAt,
            UpdatedAt = episode.UpdatedAt
        };

        return ServiceResult<EpisodeResponseDto>.Created(response, "GetEpisode", new { id = episode.Id });
    }

    public event Func<int, string, PrivacyType, Task>? EpisodeUpdated;  // id, userId, privacy
    public async Task<ServiceResult> UpdateEpisodeAsync(int id, UpdateEpisodeDto dto, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var episodeOwnership = await _episodeRepository.Query()
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new { e.PodcastId, e.Podcast.OwnerId })
            .FirstOrDefaultAsync();

        if (episodeOwnership == null)
            return ServiceResult.NotFound("Episode not found");

        if (episodeOwnership.OwnerId != userId)
            return ServiceResult.Forbidden("You can only update episodes in your own podcasts");

        var episode = await _episodeRepository.GetByIdAsync(id);
        if (episode == null)
            return ServiceResult.NotFound("Episode not found");

        episode.Title = dto.Title ?? episode.Title;
        episode.Description = dto.Description ?? episode.Description;
        episode.Privacy = dto.Privacy ?? episode.Privacy;
        episode.UpdatedAt = DateTime.UtcNow;

        await _episodeRepository.UpdateAsync(episode);

        await (EpisodeUpdated?.Invoke(id, userId, episode.Privacy) ?? Task.CompletedTask);

        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteEpisodeAsync(int id, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var episodeOwnership = await _episodeRepository.Query()
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new { e.PodcastId, e.Podcast.OwnerId })
            .FirstOrDefaultAsync();

        if (episodeOwnership == null)
            return ServiceResult.NotFound("Episode not found");

        if (episodeOwnership.OwnerId != userId)
            return ServiceResult.Forbidden("You can only delete episodes from your own podcasts");

        var episode = await _episodeRepository.GetByIdAsync(id);
        if (episode == null)
            return ServiceResult.NotFound("Episode not found");

        await _episodeRepository.DeleteByIdAsync(id);
        return ServiceResult.NoContent();
    }

    private static readonly Expression<Func<Episode, EpisodeResponseDto>> EpisodeProjection = episode => new EpisodeResponseDto
    {
        Id = episode.Id,
        Title = episode.Title,
        Description = episode.Description,
        AudioUrl = episode.AudioUrl,
        ImageUrl = episode.ImageUrl,
        Duration = episode.Duration,
        PodcastId = episode.PodcastId,
        PodcastTitle = episode.Podcast.Title,
        Privacy = episode.Privacy,
        ViewCount = episode.ViewCount,
        PublishedAt = episode.PublishedAt,
        UpdatedAt = episode.UpdatedAt
    };
}