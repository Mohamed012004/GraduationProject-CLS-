using PodcastPlatform.Models.Enums;

namespace PodcastPlatform.DTOs.Playback;

public class EpisodePlaybackDto
{
    public int EpisodeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public TimeSpan Duration { get; set; }
    public int PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public PrivacyType Privacy { get; set; }
    public DateTime PublishedAt { get; set; }
}

public class PlaybackStartDto
{
    public int EpisodeId { get; set; }
    public string? UserId { get; set; }
}

public class PlaybackStatusDto
{
    public int EpisodeId { get; set; }
    public bool IsAllowed { get; set; }
    public string? Message { get; set; }
}

public class EpisodePrivacyCheckDto
{
    public int EpisodeId { get; set; }
    public PrivacyType Privacy { get; set; }
    public bool IsAccessAllowed { get; set; }
}

