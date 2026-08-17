using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Extensions;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        return result.Status switch
        {
            LoginStatus.InvalidCredentials => Unauthorized(new { error = "Invalid email or password." }),
            LoginStatus.EmailNotVerified => Unauthorized(new
            {
                error = "Please confirm your email address before signing in."
            }),
            _ => Ok(result.Value)
        };
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (result.Status == ResultStatus.Invalid)
        {
            return BadRequest(new { error = result.Error });
        }

        // No token here on purpose: the account has to be verified before it can sign in.
        return Ok(new { message = "Account created. Check your email for a confirmation link." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        await _authService.RequestPasswordResetAsync(dto);

        // Always OK so callers cannot probe which emails are registered.
        return Ok(new { message = "If that email is registered, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);

        if (result.Status == ResultStatus.Invalid)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Your password has been reset. You can now sign in." });
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
    {
        var result = await _authService.VerifyEmailAsync(dto);

        if (result.Status == ResultStatus.Invalid)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Your email has been verified." });
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId.Value);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}
