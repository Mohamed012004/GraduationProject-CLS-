using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PodcastPlatform.DTOs.Auth;
using PodcastPlatform.Models.Entities;
using PodcastPlatform.Services.Interfaces;

namespace PodcastPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtService;
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly IPlaylistService _playlistService;

    public AuthController(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtService,
        ILogger<AuthController> logger,
        IPlaylistService playlistService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _logger = logger;
        _playlistService = playlistService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid input data"
                });

            // Check if user already exists
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Username already exists"
                });

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                });

            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                Bio = model.Bio,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning($"Registration failed for {model.UserName}: {errors}");
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = $"Registration failed: {errors}"
                });
            }

            // Create default playlists for the new user
            try
            {
                // Ensure user is persisted in database before creating playlists
                var createdUser = await _userManager.FindByIdAsync(user.Id);
                if (createdUser != null)
                {
                    await _playlistService.CreateDefaultPlaylistsAsync(createdUser.Id);
                    _logger.LogInformation($"Default playlists created for user {createdUser.Id}");
                }
                else
                {
                    _logger.LogError($"Could not find user {user.Id} after creation");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating default playlists for user {user.Id}: {ex.Message}");
            }

            var token = _jwtService.GenerateToken(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            _logger.LogInformation($"User {model.UserName} registered successfully");

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Token = token,
                User = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during registration"
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid input data"
                });

            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                _logger.LogWarning($"Login failed for {model.UserName}: Invalid credentials");
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            if (!user.IsActive)
                return Unauthorized(new AuthResponseDto
                {
                    Success = false,
                    Message = "User account is inactive"
                });

            var token = _jwtService.GenerateToken(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            _logger.LogInformation($"User {model.UserName} logged in successfully");

            return Ok(new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                User = userDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return StatusCode(500, new AuthResponseDto
            {
                Success = false,
                Message = "An error occurred during login"
            });
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                ProfileImage = user.ProfileImage,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Get profile error: {ex.Message}");
            return StatusCode(500, "An error occurred");
        }
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Optional: Get the user ID for logging purposes
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation($"User {userId} logged out successfully");
            }

            // Return 200 OK. 
            // The FRONTEND is responsible for deleting the token from localStorage.
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Logout error: {ex.Message}");
            return StatusCode(500, "An error occurred during logout");
        }
    }
    
}