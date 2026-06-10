using System.ComponentModel.DataAnnotations;
using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.Models.Entities;

public class Playlist
{
    public int Id { get; set; }

    [Required] [MaxLength(200)] public string Name { get; set; } 

    [MaxLength(1000)] public string? Description { get; set; }

    public PlaylistType Type { get; set; } = PlaylistType.Normal;

    [Required] [MaxLength(450)] public string OwnerId { get; set; } 
    public AppUser Owner { get; set; } = null!;

    public PrivacyType Privacy { get; set; } = PrivacyType.Public;

    public bool IsSystemPlaylist { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}