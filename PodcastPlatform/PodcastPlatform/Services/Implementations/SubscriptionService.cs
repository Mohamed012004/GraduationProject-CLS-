using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PodcastPlatform.DTOs.Common;
using PodcastPlatform.DTOs.Subscription;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Repositories.Interfaces;
using PodcastPlatform.Services.Interfaces;
using PodcastPlatform.Services.Models;

namespace PodcastPlatform.Services.Implementations;

public class SubscriptionService : ISubscriptionService
{
    private readonly IPodcastRepository _podcastRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, IPodcastRepository podcastRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _podcastRepository = podcastRepository;
    }

    public async Task<ServiceResult<IEnumerable<SubscriptionResponseDto>>> GetUserSubscriptionsAsync(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<IEnumerable<SubscriptionResponseDto>>.Unauthorized();

        var response = await _subscriptionRepository.Query()
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.SubscribedAt)
            .Select(SubscriptionProjection)
            .ToListAsync();

        return ServiceResult<IEnumerable<SubscriptionResponseDto>>.Ok(response);
    }

    public async Task<ServiceResult<PodcastSubscriberCountResponseDto>> GetSubscriberCountAsync(int podcastId)
    {
        var podcastExists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!podcastExists)
            return ServiceResult<PodcastSubscriberCountResponseDto>.NotFound("Podcast not found");

        var count = await _subscriptionRepository.GetSubscriberCountAsync(podcastId);
        var response = new PodcastSubscriberCountResponseDto
        {
            PodcastId = podcastId,
            SubscriberCount = count
        };

        return ServiceResult<PodcastSubscriberCountResponseDto>.Ok(response);
    }

    public async Task<ServiceResult<SubscribeResponseDto>> SubscribeAsync(int podcastId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<SubscribeResponseDto>.Unauthorized();

        var podcast = await _podcastRepository.Query()
            .AsNoTracking()
            .Where(p => p.Id == podcastId)
            .Select(p => new { p.Id, p.Title })
            .FirstOrDefaultAsync();

        if (podcast == null)
            return ServiceResult<SubscribeResponseDto>.NotFound("Podcast not found");

        var existing = await _subscriptionRepository.GetSubscriptionAsync(userId, podcastId);
        Subscription subscription;

        if (existing != null)
        {
            if (existing.IsActive)
                return ServiceResult<SubscribeResponseDto>.BadRequest("You are already subscribed to this podcast");

            existing.IsActive = true;
            existing.SubscribedAt = DateTime.UtcNow;
            await _subscriptionRepository.UpdateAsync(existing);
            subscription = existing;
        }
        else
        {
            subscription = new Subscription
            {
                UserId = userId,
                PodcastId = podcastId,
                IsActive = true
            };

            await _subscriptionRepository.AddAsync(subscription);
        }

        await _podcastRepository.IncrementSubscriberCountAsync(podcastId);

        var response = new SubscribeResponseDto
        {
            Message = "Successfully subscribed",
            SubscriptionId = subscription.Id,
            PodcastId = podcastId,
            PodcastTitle = podcast.Title,
            SubscribedAt = subscription.SubscribedAt
        };

        return ServiceResult<SubscribeResponseDto>.Ok(response);
    }

    public async Task<ServiceResult<MessageResponseDto>> UnsubscribeAsync(int podcastId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<MessageResponseDto>.Unauthorized();

        var podcastExists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!podcastExists)
            return ServiceResult<MessageResponseDto>.NotFound("Podcast not found");

        var subscription = await _subscriptionRepository.GetSubscriptionAsync(userId, podcastId);
        if (subscription == null || !subscription.IsActive)
            return ServiceResult<MessageResponseDto>.BadRequest("You are not subscribed to this podcast");

        subscription.IsActive = false;
        await _subscriptionRepository.UpdateAsync(subscription);
        await _podcastRepository.DecrementSubscriberCountAsync(podcastId);

        return ServiceResult<MessageResponseDto>.Ok(new MessageResponseDto { Message = "Successfully unsubscribed" });
    }

    public async Task<ServiceResult<SubscriptionStatusResponseDto>> IsSubscribedAsync(int podcastId, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<SubscriptionStatusResponseDto>.Unauthorized();

        var podcastExists = await _podcastRepository.Query().AsNoTracking().AnyAsync(p => p.Id == podcastId);
        if (!podcastExists)
            return ServiceResult<SubscriptionStatusResponseDto>.NotFound("Podcast not found");

        var isSubscribed = await _subscriptionRepository.IsUserSubscribedAsync(userId, podcastId);
        var response = new SubscriptionStatusResponseDto
        {
            PodcastId = podcastId,
            IsSubscribed = isSubscribed
        };

        return ServiceResult<SubscriptionStatusResponseDto>.Ok(response);
    }

    private static readonly Expression<Func<Subscription, SubscriptionResponseDto>> SubscriptionProjection = subscription => new SubscriptionResponseDto
    {
        Id = subscription.Id,
        PodcastId = subscription.PodcastId,
        PodcastTitle = subscription.Podcast.Title,
        PodcastOwnerName = subscription.Podcast.Owner.FullName,
        SubscribedAt = subscription.SubscribedAt,
        IsActive = subscription.IsActive
    };
}