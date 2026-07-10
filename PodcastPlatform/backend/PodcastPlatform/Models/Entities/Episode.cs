using System.ComponentModel.DataAnnotations;
using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.Models.Entities;

public class Episode
{
    public int Id { get; set; }

    [Required] [MaxLength(300)] public string Title { get; set; } = string.Empty;

    [MaxLength(2000)] public string? Description { get; set; }

    [Required] [MaxLength(500)] public string AudioUrl { get; set; } = string.Empty;

    [MaxLength(500)] public string? ImageUrl { get; set; }

    [Required] public TimeSpan Duration { get; set; }
   
    public int PodcastId { get; set; }
    public Podcast Podcast { get; set; } = null!;

    public PrivacyType Privacy { get; set; } = PrivacyType.Public;

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Range(0, int.MaxValue)] public int ViewCount { get; set; } = 0;

    // Navigation properties
    public ICollection<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();
}