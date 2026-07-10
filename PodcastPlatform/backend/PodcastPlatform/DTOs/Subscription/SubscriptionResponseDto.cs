namespace PodcastPlatform.DTOs.Subscription;

public class SubscriptionResponseDto
{
    public int Id { get; set; }
    public int PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public string PodcastOwnerName { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; }
    public bool IsActive { get; set; }
}

