using System.ComponentModel.DataAnnotations;

namespace PodcastPlatform.Models.Entities;

public class PodcastRating
{
    public int Id { get; set; }

    [Required] [MaxLength(450)] public string UserId { get; set; } 

    public AppUser User { get; set; } = null!;

    public int PodcastId { get; set; }
    public Podcast Podcast { get; set; } = null!;

    [Range(1, 5)] public int Rating { get; set; }

    [MaxLength(500)] public string? Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

