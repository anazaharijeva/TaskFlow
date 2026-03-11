using System.ComponentModel.DataAnnotations;

namespace TaskFlow.API.DTOs;

/// <summary>
/// DTO for user registration - contains data needed to create a new user.
/// </summary>
public record RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// DTO for user login - credentials for authentication.
/// </summary>
public record LoginDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Response returned after successful login or registration.
/// Contains JWT token and user ID for client-side storage.
/// </summary>
public record AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
}
