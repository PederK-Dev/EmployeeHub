using System.ComponentModel.DataAnnotations;

namespace EmployeeHub.Api.DTOs;

public class CreateEmployeeDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateOnly HireDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int PositionId { get; set; }
}

public class UpdateEmployeeDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateOnly HireDate { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int PositionId { get; set; }
}

public class EmployeeDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly HireDate { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public int PositionId { get; set; }

    public string PositionTitle { get; set; } = string.Empty;
}
