using System.ComponentModel.DataAnnotations;
using EmployeeHub.Api.Models;

namespace EmployeeHub.Api.DTOs;

public class CreateUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } = UserRole.Employee;

    public int? EmployeeId { get; set; }
}

public class UserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool EmailVerified { get; set; }

    public int? EmployeeId { get; set; }
}
