using PodcastPlatform.DTOs.Episode;
using PodcastPlatform.Models.Enums;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Interfaces;

public interface IEpisodeService
{
    event Func<int, string, PrivacyType, Task>? EpisodeUpdated;
    Task<ServiceResult<IEnumerable<EpisodeResponseDto>>> GetAllEpisodesAsync();
    Task<ServiceResult<EpisodeResponseDto>> GetEpisodeByIdAsync(int id, string? requesterUserId);
    Task<ServiceResult<IEnumerable<EpisodeResponseDto>>> GetEpisodesByPodcastAsync(int podcastId);
    Task<ServiceResult<EpisodeResponseDto>> CreateEpisodeAsync(CreateEpisodeDto dto, string? userId);
    Task<ServiceResult> UpdateEpisodeAsync(int id, UpdateEpisodeDto dto, string? userId);
    Task<ServiceResult> DeleteEpisodeAsync(int id, string? userId);
}