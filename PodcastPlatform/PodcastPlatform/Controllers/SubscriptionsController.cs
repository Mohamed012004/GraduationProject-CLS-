using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.DTOs.Common;
using PodcastPlatform.DTOs.Subscription;
using PodcastPlatform.Services.Interfaces;

namespace PodcastPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("my-subscriptions")]
    public async Task<ActionResult<IEnumerable<SubscriptionResponseDto>>> GetMySubscriptions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _subscriptionService.GetUserSubscriptionsAsync(userId)).ToActionResult(this);
    }

    [HttpGet("podcast/{podcastId}/subscribers")]
    [AllowAnonymous]
    public async Task<ActionResult<PodcastSubscriberCountResponseDto>> GetPodcastSubscribers(int podcastId)
    {
        return (await _subscriptionService.GetSubscriberCountAsync(podcastId)).ToActionResult(this);
    }

    [HttpPost("subscribe/{podcastId}")]
    [Authorize]
    public async Task<ActionResult<SubscribeResponseDto>> Subscribe(int podcastId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _subscriptionService.SubscribeAsync(podcastId, userId)).ToActionResult(this);
    }

    [HttpDelete("unsubscribe/{podcastId}")]
    [Authorize]
    public async Task<ActionResult<MessageResponseDto>> Unsubscribe(int podcastId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _subscriptionService.UnsubscribeAsync(podcastId, userId)).ToActionResult(this);
    }

    [HttpGet("is-subscribed/{podcastId}")]
    public async Task<ActionResult<SubscriptionStatusResponseDto>> IsSubscribed(int podcastId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _subscriptionService.IsSubscribedAsync(podcastId, userId)).ToActionResult(this);
    }
}
