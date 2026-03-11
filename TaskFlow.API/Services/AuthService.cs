using Microsoft.EntityFrameworkCore;
using TaskFlow.API.Data;
using TaskFlow.API.DTOs;
using TaskFlow.API.Models;

namespace TaskFlow.API.Services;

/// <summary>
/// Authentication service implementation.
/// Uses BCrypt for password hashing - secure and industry standard.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> ValidateUserAsync(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
            return null;

        // BCrypt.Verify compares plain password with stored hash
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already registered");

        // Hash password with BCrypt (work factor 10 = secure default)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 10);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = passwordHash,
            Name = dto.Name
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
}
