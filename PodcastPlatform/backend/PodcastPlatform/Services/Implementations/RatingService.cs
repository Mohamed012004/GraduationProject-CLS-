using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PodcastPlatform.DTOs.Rating;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Repositories.Interfaces;
using PodcastPlatform.Services.Interfaces;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Implementations;

public class RatingService : IRatingService
{
    private readonly IPodcastRepository _podcastRepository;
    private readonly IRatingRepository _ratingRepository;

    public RatingService(IRatingRepository ratingRepository, IPodcastRepository podcastRepository)
    {
        _ratingRepository = ratingRepository;
        _podcastRepository = podcastRepository;
    }

    public async Task<ServiceResult<IEnumerable<RatingResponseDto>>> GetPodcastRatingsAsync(int podcastId)
    {
        var podcastExists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!podcastExists)
            return ServiceResult<IEnumerable<RatingResponseDto>>.NotFound("Podcast not found");

        var response = await _ratingRepository.Query()
            .AsNoTracking()
            .Where(r => r.PodcastId == podcastId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(RatingProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<RatingResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<PodcastRatingStatsDto>> GetPodcastRatingStatsAsync(int podcastId)
    {
        var podcast = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == podcastId)
            .Select(p => new { p.Id, p.Title })
            .FirstOrDefaultAsync();

        if (podcast == null)
            return ServiceResult<PodcastRatingStatsDto>.NotFound("Podcast not found");

        var ratingsQuery = _ratingRepository.Query().AsNoTracking().Where(r => r.PodcastId == podcastId);
        var count = await ratingsQuery.CountAsync();
        var average = count == 0 ? 0 : await ratingsQuery.AverageAsync(r => (double)r.Rating);

        var stats = new PodcastRatingStatsDto
        {
            PodcastId = podcastId,
            PodcastTitle = podcast.Title,
            AverageRating = average,
            TotalRatings = count,
            RatingCount1 = await ratingsQuery.CountAsync(r => r.Rating == 1),
            RatingCount2 = await ratingsQuery.CountAsync(r => r.Rating == 2),
            RatingCount3 = await ratingsQuery.CountAsync(r => r.Rating == 3),
            RatingCount4 = await ratingsQuery.CountAsync(r => r.Rating == 4),
            RatingCount5 = await ratingsQuery.CountAsync(r => r.Rating == 5)
        };

        return ServiceResult<PodcastRatingStatsDto>.Ok(stats);
    }

    public async Task<ServiceResult<RatingResponseDto>> GetUserRatingAsync(int podcastId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<RatingResponseDto>.Unauthorized();

        var podcastExists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!podcastExists)
            return ServiceResult<RatingResponseDto>.NotFound("Podcast not found");

        var rating = await _ratingRepository.Query()
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.PodcastId == podcastId)
            .Select(RatingProjection)
            .FirstOrDefaultAsync();

        if (rating == null)
            return ServiceResult<RatingResponseDto>.NotFound("You haven't rated this podcast");

        return ServiceResult<RatingResponseDto>.Ok(rating);
    }

    public async Task<ServiceResult<RatingResponseDto>> CreateRatingAsync(CreateRatingDto dto, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<RatingResponseDto>.Unauthorized();

        var podcast = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == dto.PodcastId)
            .Select(p => new { p.Id, p.Title })
            .FirstOrDefaultAsync();

        if (podcast == null)
            return ServiceResult<RatingResponseDto>.NotFound("Podcast not found");

        var existing = await _ratingRepository.Query().AsNoTracking().AnyAsync(r => r.UserId == userId && r.PodcastId == dto.PodcastId);
        if (existing)
            return ServiceResult<RatingResponseDto>.BadRequest("User has already rated this podcast");

        var rating = new PodcastRating
        {
            UserId = userId,
            PodcastId = dto.PodcastId,
            Rating = dto.Rating,
            Review = dto.Review,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.AddAsync(rating);
        var response = await _ratingRepository.Query()
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.PodcastId == dto.PodcastId)
            .Select(RatingProjection)
            .FirstOrDefaultAsync() ?? new RatingResponseDto
        {
            Id = rating.Id,
            UserId = rating.UserId,
            UserName = string.Empty,
            PodcastId = rating.PodcastId,
            PodcastTitle = podcast.Title,
            Rating = rating.Rating,
            Review = rating.Review,
            CreatedAt = rating.CreatedAt,
            UpdatedAt = rating.UpdatedAt
        };

        return ServiceResult<RatingResponseDto>.Created(response, "GetPodcastRatings", new { podcastId = dto.PodcastId });
    }

    public async Task<ServiceResult> UpdateRatingAsync(int podcastId, UpdateRatingDto dto, string? userId)
    {
        Console.WriteLine($"UserId = {userId}");
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var rating = await _ratingRepository.Query()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.PodcastId == podcastId);

        if (rating == null)
            return ServiceResult.NotFound("Rating not found");

        if (dto.Rating.HasValue)
            rating.Rating = dto.Rating.Value;

        if (!string.IsNullOrWhiteSpace(dto.Review))
            rating.Review = dto.Review;

        rating.UpdatedAt = DateTime.UtcNow;

        await _ratingRepository.UpdateAsync(rating);
        return ServiceResult.NoContent();
    }

    public async Task<ServiceResult> DeleteRatingAsync(int id, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Unauthorized();

        var rating = await _ratingRepository.Query().FirstOrDefaultAsync(r => r.Id == id);
        if (rating == null)
            return ServiceResult.NotFound("Rating not found");

        if (rating.UserId != userId)
            return ServiceResult.Forbidden("You can only delete your own ratings");

        var success = await _ratingRepository.DeleteByIdAsync(id);
        if (!success)
            return ServiceResult.NotFound("Rating not found");

        return ServiceResult.NoContent();
    }

    private static readonly Expression<Func<PodcastRating, RatingResponseDto>> RatingProjection = rating => new RatingResponseDto
    {
        Id = rating.Id,
        UserId = rating.UserId,
        UserName = rating.User.UserName ?? string.Empty,
        PodcastId = rating.PodcastId,
        PodcastTitle = rating.Podcast.Title,
        Rating = rating.Rating,
        Review = rating.Review,
        CreatedAt = rating.CreatedAt,
        UpdatedAt = rating.UpdatedAt
    };
}