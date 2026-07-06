using System.ComponentModel.DataAnnotations;

namespace EmployeeHub.Api.DTOs;

public class UpdateDepartmentDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
