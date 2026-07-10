using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace PodcastPlatform.Models.Entities;

public class AppUser : IdentityUser
{
    [Required] [MaxLength(200)] public string FullName { get; set; } 

    [MaxLength(500)] public string? ProfileImage { get; set; }

    [MaxLength(1000)] public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
    
    

    // Navigation properties
    public ICollection<Podcast> OwnedPodcasts { get; set; } = new List<Podcast>();
    public ICollection<Playlist> OwnedPlaylists { get; set; } = new List<Playlist>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<PodcastRating> Ratings { get; set; } = new List<PodcastRating>();
    
}

