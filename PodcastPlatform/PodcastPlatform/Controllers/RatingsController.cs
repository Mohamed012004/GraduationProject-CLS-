using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.DTOs.Rating;
using PodcastPlatform.Services.Interfaces;

namespace PodcastPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpGet("podcast/{podcastId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RatingResponseDto>>> GetPodcastRatings(int podcastId)
    {
        return (await _ratingService.GetPodcastRatingsAsync(podcastId)).ToActionResult(this);
    }

    [HttpGet("podcast/{podcastId}/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<PodcastRatingStatsDto>> GetPodcastRatingStats(int podcastId)
    {
        return (await _ratingService.GetPodcastRatingStatsAsync(podcastId)).ToActionResult(this);
    }

    [HttpGet("my-rating/{podcastId}")]
    [Authorize]
    public async Task<ActionResult<RatingResponseDto>> GetMyRating(int podcastId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _ratingService.GetUserRatingAsync(podcastId, userId)).ToActionResult(this);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RatingResponseDto>> CreateRating(CreateRatingDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _ratingService.CreateRatingAsync(dto, userId)).ToActionResult(this);
    }

    [HttpPatch("{podcastId}")]
    [Authorize]
    public async Task<IActionResult> UpdateRating(int podcastId, UpdateRatingDto dto)
    
    {
        Console.WriteLine("Controller reached");
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _ratingService.UpdateRatingAsync(podcastId, dto, userId)).ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteRating(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _ratingService.DeleteRatingAsync(id, userId)).ToActionResult(this);
    }
}