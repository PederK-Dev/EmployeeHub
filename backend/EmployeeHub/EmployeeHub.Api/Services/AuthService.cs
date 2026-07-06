using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class AuthService
{
    private readonly EmployeeHubDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthService(
        EmployeeHubDbContext context,
        IPasswordHasher<User> passwordHasher,
        TokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToDto(user)
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        return user is null ? null : ToDto(user);
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            EmployeeId = user.EmployeeId
        };
    }
}
