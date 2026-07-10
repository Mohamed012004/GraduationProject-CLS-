using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.DTOs.Podcast;
using PodcastPlatform.Services.Interfaces;

namespace PodcastPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PodcastsController : ControllerBase
{
    private readonly IPodcastService _podcastService;

    public PodcastsController(IPodcastService podcastService)
    {
        _podcastService = podcastService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PodcastResponseDto>>> GetAllPodcasts()
    {
        return (await _podcastService.GetAllPodcastsAsync()).ToActionResult(this);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<PodcastResponseDto>> GetPodcast(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _podcastService.GetPodcastByIdAsync(id, userId)).ToActionResult(this);
    }

    [HttpGet("owner/{ownerId}")]
    public async Task<ActionResult<IEnumerable<PodcastResponseDto>>> GetPodcastsByOwner(string ownerId)
    {
        return (await _podcastService.GetPodcastsByOwnerAsync(ownerId)).ToActionResult(this);
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PodcastResponseDto>> CreatePodcast([FromForm] CreatePodcastDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _podcastService.CreatePodcastAsync(dto, userId)).ToActionResult(this);
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePodcast(int id, UpdatePodcastDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _podcastService.UpdatePodcastAsync(id, dto, userId)).ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePodcast(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _podcastService.DeletePodcastAsync(id, userId)).ToActionResult(this);
    }

    [HttpGet("{id}/subscribers")]
    public async Task<ActionResult<int>> GetSubscriberCount(int id)
    {
        return (await _podcastService.GetSubscriberCountAsync(id)).ToActionResult(this);
    }
}