using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

/// <summary>
/// Service for generating JWT tokens for authenticated users.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token containing user claims (e.g., userId, email).
    /// </summary>
    string GenerateToken(User user);
}
