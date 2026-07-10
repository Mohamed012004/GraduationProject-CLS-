using System.ComponentModel.DataAnnotations;
using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.DTOs.Podcast;

public class CreatePodcastDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)] public string? Description { get; set; }

    [StringLength(200)] public string? Category { get; set; }

  

    public IFormFile? ImageFile { get; set; }

    public PrivacyType Privacy { get; set; } = PrivacyType.Public;
}

public class UpdatePodcastDto
{
    [StringLength(200, MinimumLength = 3)] public string? Title { get; set; }

    [StringLength(2000)] public string? Description { get; set; }

    [StringLength(200)] public string? Category { get; set; }

    public PrivacyType? Privacy { get; set; }
}

public class PodcastResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }
    public UserDto? Owner { get; set; }
    public PrivacyType Privacy { get; set; }
    public int SubscriberCount { get; set; }
    public int EpisodeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

}