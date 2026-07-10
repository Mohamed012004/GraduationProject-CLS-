using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.DTOs.Playlist;
using PodcastPlatform.Services.Interfaces;

namespace PodcastPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistsController : ControllerBase
{
    private readonly IPlaylistService _playlistService;

    public PlaylistsController(IPlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlaylistResponseDto>>> GetAllPlaylists()
    {
        return (await _playlistService.GetAllPlaylistsAsync()).ToActionResult(this);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlaylistResponseDto>> GetPlaylist(int id)
    {
        return (await _playlistService.GetPlaylistByIdAsync(id)).ToActionResult(this);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PlaylistResponseDto>>> GetUserPlaylists(string userId)
    {
        return (await _playlistService.GetPlaylistsByOwnerAsync(userId)).ToActionResult(this);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PlaylistResponseDto>> CreatePlaylist(CreatePlaylistDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _playlistService.CreatePlaylistAsync(dto, userId)).ToActionResult(this);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdatePlaylist(int id, UpdatePlaylistDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _playlistService.UpdatePlaylistAsync(id, dto, userId)).ToActionResult(this);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePlaylist(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _playlistService.DeletePlaylistAsync(id, userId)).ToActionResult(this);
    }

    [HttpPost("{playlistId}/episodes/{episodeId}")]
    [Authorize]
    public async Task<IActionResult> AddEpisodeToPlaylist(int playlistId, int episodeId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _playlistService.AddEpisodeToPlaylistAsync(playlistId, episodeId, userId)).ToActionResult(this);
    }

    [HttpDelete("{playlistId}/episodes/{episodeId}")]
    [Authorize]
    public async Task<IActionResult> RemoveEpisodeFromPlaylist(int playlistId, int episodeId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return (await _playlistService.RemoveEpisodeFromPlaylistAsync(playlistId, episodeId, userId)).ToActionResult(this);
    }
}