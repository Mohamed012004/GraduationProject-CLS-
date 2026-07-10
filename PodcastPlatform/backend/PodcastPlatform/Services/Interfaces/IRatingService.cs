using PodcastPlatform.DTOs.Rating;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Interfaces;

public interface IRatingService
{
    Task<ServiceResult<IEnumerable<RatingResponseDto>>> GetPodcastRatingsAsync(int podcastId);
    Task<ServiceResult<PodcastRatingStatsDto>> GetPodcastRatingStatsAsync(int podcastId);
    Task<ServiceResult<RatingResponseDto>> GetUserRatingAsync(int podcastId, string? userId);
    Task<ServiceResult<RatingResponseDto>> CreateRatingAsync(CreateRatingDto dto, string? userId);
    Task<ServiceResult> UpdateRatingAsync(int podcastId, UpdateRatingDto dto, string? userId);
    Task<ServiceResult> DeleteRatingAsync(int id, string? userId);
}