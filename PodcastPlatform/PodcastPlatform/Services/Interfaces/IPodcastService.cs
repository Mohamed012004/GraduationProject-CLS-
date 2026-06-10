using PodcastPlatform.DTOs.Podcast;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Interfaces;


public interface IPodcastService
{
    Task<ServiceResult<IEnumerable<PodcastResponseDto>>> GetAllPodcastsAsync();
    Task<ServiceResult<PodcastResponseDto>> GetPodcastByIdAsync(int id, string? requesterUserId);
    Task<ServiceResult<IEnumerable<PodcastResponseDto>>> GetPodcastsByOwnerAsync(string ownerId);
    Task<ServiceResult<PodcastResponseDto>> CreatePodcastAsync(CreatePodcastDto dto, string? ownerId);
    Task<ServiceResult> UpdatePodcastAsync(int id, UpdatePodcastDto dto, string? ownerId);
    Task<ServiceResult> DeletePodcastAsync(int id, string? ownerId);
    Task<ServiceResult<int>> GetSubscriberCountAsync(int podcastId);
}
