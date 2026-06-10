using System.ComponentModel.DataAnnotations;

namespace PodcastPlatform.Models.Entities;

public class Subscription
{
    public int Id { get; set; }

    [Required] [MaxLength(450)] public string UserId { get; set; } 

    public AppUser User { get; set; } = null!;

    public int PodcastId { get; set; }
    public Podcast Podcast { get; set; } = null!;

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}