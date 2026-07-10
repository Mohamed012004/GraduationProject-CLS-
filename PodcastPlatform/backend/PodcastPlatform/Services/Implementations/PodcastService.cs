using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PodcastPlatform.DTOs.Podcast;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Repositories.Interfaces;
using PodcastPlatform.Services.Interfaces;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Implementations;

public class PodcastService : IPodcastService
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IPodcastRepository _podcastRepository;

    public PodcastService(IPodcastRepository podcastRepository, ICloudinaryService cloudinaryService)
    {
        _podcastRepository = podcastRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<ServiceResult<IEnumerable<PodcastResponseDto>>> GetAllPodcastsAsync()
    {
        var response = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Privacy == PrivacyType.Public)
            .OrderByDescending(p => p.CreatedAt)
            .Select(PodcastProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<PodcastResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<PodcastResponseDto>> GetPodcastByIdAsync(int id, string? requesterUserId)
    {
        var podcast = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == id)
           .Select(PodcastProjection)
            .FirstOrDefaultAsync();

        if (podcast == null)
            return ServiceResult<PodcastResponseDto>.NotFound("Podcast not found");

        if (podcast.Privacy == PrivacyType.Private && podcast.Owner.Id != requesterUserId)
            return ServiceResult<PodcastResponseDto>.Forbidden();

        return ServiceResult<PodcastResponseDto>.Ok(podcast);
    }

    public async Task<ServiceResult<IEnumerable<PodcastResponseDto>>> GetPodcastsByOwnerAsync(string ownerId)
    {
        var response = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(PodcastProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<PodcastResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<PodcastResponseDto>> CreatePodcastAsync(CreatePodcastDto dto, string? ownerId)
    {
        if (string.IsNullOrEmpty(ownerId))
            return ServiceResult<PodcastResponseDto>.Unauthorized();

        string? imageUrl = null;
        if (dto.ImageFile != null)
            imageUrl = await _cloudinaryService.UploadImageAsync(dto.ImageFile);

        var podcast = new Podcast
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            ImageUrl = imageUrl,
            OwnerId = ownerId,
            Privacy = dto.Privacy,
            CreatedAt = DateTime.UtcNow
        };

        await _podcastRepository.AddAsync(podcast);

        var response = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == podcast.Id)
            .Select(PodcastProjection)
            .FirstOrDefaultAsync();

        if (response == null)
        {
            response = new PodcastResponseDto
            {
                Id = podcast.Id,
                Title = podcast.Title,
                Description = podcast.Description,
                Category = podcast.Category,
                ImageUrl = podcast.ImageUrl,
                Owner = new UserDto { Id = ownerId, UserName = string.Empty },
                Privacy = podcast.Privacy,
                SubscriberCount = podcast.SubscriberCount,
                EpisodeCount = 0,
                CreatedAt = podcast.CreatedAt,
                UpdatedAt = podcast.UpdatedAt
            };
        }

        return ServiceResult<PodcastResponseDto>.Created(response, "GetPodcast", new { id = podcast.Id });
    }

    public async Task<ServiceResult> UpdatePodcastAsync(int id, UpdatePodcastDto dto, string? ownerId)
    {
        if (string.IsNullOrEmpty(ownerId))
            return ServiceResult.Unauthorized();

        var existingPodcast = await _podcastRepository.GetByIdAsync(id);
        if (existingPodcast == null)
            return ServiceResult.NotFound("Podcast not found");

        if (existingPodcast.OwnerId != ownerId)
            return ServiceResult.Forbidden();

        existingPodcast.Title = dto.Title ?? existingPodcast.Title;
        existingPodcast.Description = dto.Description ?? existingPodcast.Description;
        existingPodcast.Category = dto.Category ?? existingPodcast.Category;
        existingPodcast.Privacy = dto.Privacy ?? existingPodcast.Privacy;
        existingPodcast.UpdatedAt = DateTime.UtcNow;

        await _podcastRepository.UpdateAsync(existingPodcast);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeletePodcastAsync(int id, string? ownerId)
    {
        if (string.IsNullOrEmpty(ownerId))
            return ServiceResult.Unauthorized();

        var podcast = await _podcastRepository.GetByIdAsync(id);
        if (podcast == null)
            return ServiceResult.NotFound("Podcast not found");

        if (podcast.OwnerId != ownerId)
            return ServiceResult.Forbidden();

        await _podcastRepository.DeleteByIdAsync(id);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult<int>> GetSubscriberCountAsync(int podcastId)
    {
        var exists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!exists)
            return ServiceResult<int>.NotFound("Podcast not found");

        var count = await _podcastRepository.GetSubscriberCountAsync(podcastId);
        return ServiceResult<int>.Ok(count);
    }

    private static readonly Expression<Func<Podcast, PodcastResponseDto>> PodcastProjection = podcast => new PodcastResponseDto
    {
        Id = podcast.Id,
        Title = podcast.Title,
        Description = podcast.Description,
        Category = podcast.Category,
        ImageUrl = podcast.ImageUrl,
        Owner = new UserDto
        {
            Id = podcast.Owner.Id,
            UserName = podcast.Owner.UserName ?? string.Empty
        },
        Privacy = podcast.Privacy,
        SubscriberCount = podcast.SubscriberCount,
        EpisodeCount = podcast.Episodes.Count,
        CreatedAt = podcast.CreatedAt,
        UpdatedAt = podcast.UpdatedAt
    };
}