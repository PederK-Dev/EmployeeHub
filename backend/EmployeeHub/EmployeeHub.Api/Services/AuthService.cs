using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class AuthService
{
    /// <summary>
    /// A valid hash of a password nobody holds, used to spend the same time verifying a login for
    /// an unknown email as for a real one. Built once per process on first use.
    /// </summary>
    private static string? _dummyHash;

    private readonly EmployeeHubDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly TokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public AuthService(
        EmployeeHubDbContext context,
        IPasswordHasher<User> passwordHasher,
        TokenService tokenService,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<LoginResult> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // Verify against a throwaway hash when the email is unknown, so that a failed login costs
        // the same either way. Returning early here would leak which emails are registered.
        if (user is null)
        {
            _passwordHasher.VerifyHashedPassword(new User(), DummyHash, dto.Password);
            return LoginResult.Failed();
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return LoginResult.Failed();
        }

        // Self-registered accounts stay inert until the address is confirmed, so registering
        // alone is not enough to obtain a token.
        if (!user.EmailVerified)
        {
            return LoginResult.Unverified();
        }

        return LoginResult.Success(BuildAuthResponse(user));
    }

    /// <summary>
    /// Creates an unverified <see cref="UserRole.Employee"/> account and emails a confirmation
    /// link. Deliberately returns no token — the account cannot sign in until it is verified,
    /// and an admin still has to link it to an employee record before it can do much.
    /// </summary>
    public async Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return ServiceResult<bool>.Invalid($"A user with email '{dto.Email}' already exists.");
        }

        var user = new User
        {
            Email = dto.Email,
            Role = UserRole.Employee,
            EmailVerified = false,
            EmailVerificationToken = TokenGenerator.Create()
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SendVerificationEmailAsync(user);

        return ServiceResult<bool>.Ok(true);
    }

    /// <summary>
    /// Always succeeds from the caller's perspective (to avoid leaking which emails are registered).
    /// If the email matches a user, a reset token is generated and emailed.
    /// </summary>
    public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
        {
            return;
        }

        user.PasswordResetToken = TokenGenerator.Create();
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        var link = $"{ClientBaseUrl()}/reset-password?token={Uri.EscapeDataString(user.PasswordResetToken)}";
        await _emailSender.SendAsync(
            user.Email,
            "Reset your EmployeeHub password",
            $"Use the link below to reset your password. It expires in 1 hour.\n{link}");
    }

    public async Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == dto.Token);
        if (user is null || user.PasswordResetTokenExpiresAt is null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult<bool>.Invalid("This reset link is invalid or has expired.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == dto.Token);
        if (user is null)
        {
            return ServiceResult<bool>.Invalid("This verification link is invalid.");
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        return user is null ? null : ToDto(user);
    }

    private string DummyHash =>
        _dummyHash ??= _passwordHasher.HashPassword(new User(), "not-a-real-password");

    private AuthResponseDto BuildAuthResponse(User user)
    {
        var (token, expiresAt) = _tokenService.CreateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = ToDto(user)
        };
    }

    private async Task SendVerificationEmailAsync(User user)
    {
        var link = $"{ClientBaseUrl()}/verify-email?token={Uri.EscapeDataString(user.EmailVerificationToken!)}";
        await _emailSender.SendAsync(
            user.Email,
            "Verify your EmployeeHub email",
            $"Welcome to EmployeeHub! Confirm your email using the link below.\n{link}");
    }

    private string ClientBaseUrl()
    {
        return _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?.FirstOrDefault()
               ?? "http://localhost:5173";
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            EmailVerified = user.EmailVerified,
            EmployeeId = user.EmployeeId
        };
    }
}

public enum LoginStatus
{
    Success,

    /// <summary>Unknown email or wrong password — deliberately not distinguished.</summary>
    InvalidCredentials,

    /// <summary>Correct credentials, but the email address has not been confirmed yet.</summary>
    EmailNotVerified
}

/// <summary>Outcome of a sign-in attempt.</summary>
public record LoginResult(LoginStatus Status, AuthResponseDto? Value = null)
{
    public static LoginResult Success(AuthResponseDto value) => new(LoginStatus.Success, value);

    public static LoginResult Failed() => new(LoginStatus.InvalidCredentials);

    public static LoginResult Unverified() => new(LoginStatus.EmailNotVerified);
}
