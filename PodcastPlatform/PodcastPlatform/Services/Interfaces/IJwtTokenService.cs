using PodcastPlatform.Models.Entities;

namespace PodcastPlatform.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(AppUser user);
    bool ValidateToken(string token);
}