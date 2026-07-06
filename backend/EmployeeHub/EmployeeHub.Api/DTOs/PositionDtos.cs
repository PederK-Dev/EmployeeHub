using System.ComponentModel.DataAnnotations;

namespace EmployeeHub.Api.DTOs;

public class CreatePositionDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdatePositionDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}

public class PositionDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
}
