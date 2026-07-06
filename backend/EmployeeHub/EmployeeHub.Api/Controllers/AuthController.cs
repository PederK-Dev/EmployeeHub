using System.Security.Claims;
using EmployeeHub.Api.DTOs;
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

        if (result is null)
        {
            return Unauthorized(new { error = "Invalid email or password." });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me()
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

        if (!int.TryParse(subject, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }
}
