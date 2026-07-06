namespace EmployeeHub.Api.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Employee;

    public bool EmailVerified { get; set; }

    public string? EmailVerificationToken { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public int? EmployeeId { get; set; }

    public Employee? Employee { get; set; }
}
