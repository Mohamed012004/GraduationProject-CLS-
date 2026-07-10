using System.ComponentModel.DataAnnotations;
using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.DTOs.Playlist;

public class CreatePlaylistDto
{
    [Required(ErrorMessage = "Playlist name is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)] public string? Description { get; set; }
    

    public PrivacyType Privacy { get; set; } = PrivacyType.Public;
}

public class UpdatePlaylistDto
{
    [StringLength(200, MinimumLength = 1)] public string? Name { get; set; }

    [StringLength(1000)] public string? Description { get; set; }

    public PrivacyType? Privacy { get; set; }
}

public class PlaylistResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlaylistType Type { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public PrivacyType Privacy { get; set; }
    public bool IsSystemPlaylist { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddEpisodeToPlaylistDto
{
    [Required(ErrorMessage = "Episode ID is required")]
    public int EpisodeId { get; set; }
}