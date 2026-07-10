namespace PodcastPlatform.DTOs.Subscription;

public class SubscribeResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public int PodcastId { get; set; }
    public string PodcastTitle { get; set; } = string.Empty;
    public DateTime SubscribedAt { get; set; }
}

