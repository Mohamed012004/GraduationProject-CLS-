namespace PodcastPlatform.Models.Entities;

public class PlaylistItem
{
    public int Id { get; set; }

    public int PlaylistId { get; set; }
    public Playlist Playlist { get; set; } = null!;

    public int EpisodeId { get; set; }
    public Episode Episode { get; set; } = null!;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}