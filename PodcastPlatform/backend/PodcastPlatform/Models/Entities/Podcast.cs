using System.ComponentModel.DataAnnotations;
using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.Models.Entities;

public class Podcast
{
    public int Id { get; set; }

    [Required] [MaxLength(200)] public string Title { get; set; } 

    [MaxLength(2000)] public string? Description { get; set; }

    [MaxLength(200)] public string? Category { get; set; }

    [MaxLength(500)] public string? ImageUrl { get; set; }

    [Required] [MaxLength(450)] public string OwnerId { get; set; }

    public AppUser Owner { get; set; } = null!;

    [Required] public PrivacyType Privacy { get; set; } = PrivacyType.Public;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Range(0, int.MaxValue)] public int SubscriberCount { get; set; } = 0;

    // Navigation properties
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<PodcastRating> Ratings { get; set; } = new List<PodcastRating>();
}

