using System.ComponentModel.DataAnnotations;

namespace EmployeeHub.Api.DTOs;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public UserDto User { get; set; } = null!;
}
