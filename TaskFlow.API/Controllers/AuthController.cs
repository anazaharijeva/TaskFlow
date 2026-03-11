using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;
using TaskFlow.API.Services;

namespace TaskFlow.API.Controllers;

/// <summary>
/// Authentication controller - handles login and registration.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _db;

    public AuthController(IAuthService authService, ITokenService tokenService, AppDbContext db)
    {
        _authService = authService;
        _tokenService = tokenService;
        _db = db;
    }

    /// <summary>
    /// GET /api/auth/me - Get current user info (for restoring session).
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return Unauthorized();

        return Ok(new { userId = user.Id.ToString(), userName = user.Name });
    }

    /// <summary>
    /// POST /api/auth/login - Authenticate user and return JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _authService.ValidateUserAsync(dto);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id.ToString(),
            UserName = user.Name
        });
    }

    /// <summary>
    /// POST /api/auth/register - Create new user account.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto);
        var token = _tokenService.GenerateToken(user);

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id.ToString(),
            UserName = user.Name
        });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
        {
            return BadRequest(new { message = "Email already registered" });
        }
    }
}
