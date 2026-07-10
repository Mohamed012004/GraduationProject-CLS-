using PodcastPlatform.DTOs.Common;
using PodcastPlatform.DTOs.Subscription;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Interfaces;

public interface ISubscriptionService
{
    Task<ServiceResult<IEnumerable<SubscriptionResponseDto>>> GetUserSubscriptionsAsync(string? userId);
    Task<ServiceResult<PodcastSubscriberCountResponseDto>> GetSubscriberCountAsync(int podcastId);
    Task<ServiceResult<SubscribeResponseDto>> SubscribeAsync(int podcastId, string? userId);
    Task<ServiceResult<MessageResponseDto>> UnsubscribeAsync(int podcastId, string? userId);
    Task<ServiceResult<SubscriptionStatusResponseDto>> IsSubscribedAsync(int podcastId, string? userId);
}