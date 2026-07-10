using System.ComponentModel.DataAnnotations;

namespace PodcastPlatform.DTOs.Rating;

public class CreateRatingDto
{
    [Required(ErrorMessage = "Podcast ID is required")]
    public int PodcastId { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; }

    [StringLength(500)] public string? Review { get; set; }
}

public class UpdateRatingDto
{
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int? Rating { get; set; }

    [StringLength(500)] public string? Review { get; set; }
}

public class RatingResponseDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Review { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PodcastRatingStatsDto
{
    public int PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int RatingCount1 { get; set; }
    public int RatingCount2 { get; set; }
    public int RatingCount3 { get; set; }
    public int RatingCount4 { get; set; }
    public int RatingCount5 { get; set; }
}