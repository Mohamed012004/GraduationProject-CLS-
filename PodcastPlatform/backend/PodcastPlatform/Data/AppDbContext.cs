using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PodcastPlatform.Models.Entities;



namespace PodcastPlatform.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 🧱 DbSets (Tables)
    public DbSet<Podcast> Podcasts { get; set; }
    public DbSet<Episode> Episodes { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistItem> PlaylistItems { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<PodcastRating> PodcastRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ============ PODCAST RELATIONSHIPS ============

        builder.Entity<Podcast>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedPodcasts)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        

        builder.Entity<Podcast>()
            .HasMany(p => p.Episodes)
            .WithOne(e => e.Podcast)
            .HasForeignKey(e => e.PodcastId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Podcast>()
            .HasMany(p => p.Subscriptions)
            .WithOne(s => s.Podcast)
            .HasForeignKey(s => s.PodcastId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Podcast>()
            .HasMany(p => p.Ratings)
            .WithOne(r => r.Podcast)
            .HasForeignKey(r => r.PodcastId)
            .OnDelete(DeleteBehavior.NoAction);

        // ============ EPISODE RELATIONSHIPS ============

        builder.Entity<Episode>()
            .HasMany(e => e.PlaylistItems)
            .WithOne(pi => pi.Episode)
            .HasForeignKey(pi => pi.EpisodeId)
            .OnDelete(DeleteBehavior.NoAction); // 🔥 FIX

        // ============ PLAYLIST RELATIONSHIPS ============

        builder.Entity<Playlist>()
            .HasOne(p => p.Owner)
            .WithMany(u => u.OwnedPlaylists)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Playlist>()
            .HasMany(p => p.Items)
            .WithOne(pi => pi.Playlist)
            .HasForeignKey(pi => pi.PlaylistId)
            .OnDelete(DeleteBehavior.NoAction); // 🔥 FIX

        // ============ SUBSCRIPTION RELATIONSHIPS ============

        builder.Entity<Subscription>()
            .HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Subscription>()
            .HasIndex(s => new { s.UserId, s.PodcastId })
            .IsUnique();

        // ============ PLAYLISTITEM CONSTRAINTS ============

        builder.Entity<PlaylistItem>()
            .HasIndex(pi => new { pi.PlaylistId, pi.EpisodeId })
            .IsUnique();

        // ============ PODCASTRATING RELATIONSHIPS ============

        builder.Entity<PodcastRating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<PodcastRating>()
            .HasIndex(r => new { r.UserId, r.PodcastId })
            .IsUnique();

        // ============ INDEX CONFIGURATIONS ============

        builder.Entity<Podcast>()
            .HasIndex(p => p.OwnerId);

        builder.Entity<Episode>()
            .HasIndex(e => e.PodcastId);

        builder.Entity<Playlist>()
            .HasIndex(p => p.OwnerId);

        builder.Entity<PlaylistItem>()
            .HasIndex(pi => pi.PlaylistId);

        builder.Entity<Subscription>()
            .HasIndex(s => s.UserId);
    }
}