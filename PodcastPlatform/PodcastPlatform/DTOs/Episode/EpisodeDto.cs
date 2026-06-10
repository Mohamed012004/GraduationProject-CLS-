using System.ComponentModel.DataAnnotations;
using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.DTOs.Episode;

public class CreateEpisodeDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(300, MinimumLength = 3)]
    public string Title { get; set; } 

    [StringLength(2000)] public string? Description { get; set; }

    [Required]
    public IFormFile AudioFile { get; set; } = null!;

    public IFormFile? ImageFile { get; set; }
    

    [Required(ErrorMessage = "Podcast ID is required")]
    public int PodcastId { get; set; }

    public PrivacyType Privacy { get; set; } = PrivacyType.Public;
}

public class UpdateEpisodeDto
{ 
    
    [StringLength(300, MinimumLength = 3)]
    public string? Title { get; set; }

    [StringLength(2000)] public string? Description { get; set; }

    public PrivacyType? Privacy { get; set; }
}

public class EpisodeResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AudioUrl { get; set; }
    public string? ImageUrl { get; set; }
    public TimeSpan Duration { get; set; }
    public int PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public PrivacyType Privacy { get; set; }
    public int ViewCount { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}