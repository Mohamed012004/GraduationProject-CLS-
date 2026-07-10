using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.DTOs.Episode;
using PodcastPlatform.Services.Interfaces;

namespace PodcastPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpisodesController : ControllerBase
{
    private readonly IEpisodeService _episodeService;

    public EpisodesController(IEpisodeService episodeService)
    {
        _episodeService = episodeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EpisodeResponseDto>>> GetAllEpisodes()
    {
        return (await _episodeService.GetAllEpisodesAsync()).ToActionResult(this);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<EpisodeResponseDto>> GetEpisode(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _episodeService.GetEpisodeByIdAsync(id, userId)).ToActionResult(this);
    }

    [HttpGet("podcast/{podcastId}")]
    public async Task<ActionResult<IEnumerable<EpisodeResponseDto>>> GetEpisodesByPodcast(int podcastId)
    {
        return (await _episodeService.GetEpisodesByPodcastAsync(podcastId)).ToActionResult(this);
    }
    [HttpGet("playlist/{playlistId}")]
    public async Task<ActionResult<IEnumerable<EpisodeResponseDto>>> GetEpisodesByPlaylist(int playlistId)
    {
        return (await _episodeService.GetEpisodesByPodcastAsync(playlistId)).ToActionResult(this);
    }

    [HttpPost]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<EpisodeResponseDto>> CreateEpisode([FromForm] CreateEpisodeDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _episodeService.CreateEpisodeAsync(dto, userId)).ToActionResult(this);
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateEpisode(int id, UpdateEpisodeDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _episodeService.UpdateEpisodeAsync(id, dto, userId)).ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteEpisode(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _episodeService.DeleteEpisodeAsync(id, userId)).ToActionResult(this);
    }
}