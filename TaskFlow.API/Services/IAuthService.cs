using TaskFlow.API.DTOs;
using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

/// <summary>
/// Authentication service - handles user registration and login validation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates user credentials and returns the user if valid.
    /// </summary>
    Task<User?> ValidateUserAsync(LoginDto dto);

    /// <summary>
    /// Registers a new user and returns the created user.
    /// </summary>
    Task<User> RegisterAsync(RegisterDto dto);
}
