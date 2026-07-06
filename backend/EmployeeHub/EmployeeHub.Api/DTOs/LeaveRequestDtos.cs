using System.ComponentModel.DataAnnotations;
using EmployeeHub.Api.Models;

namespace EmployeeHub.Api.DTOs;

public class CreateLeaveRequestDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public LeaveType Type { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

public class UpdateLeaveRequestStatusDto
{
    [Required]
    public LeaveStatus Status { get; set; }
}

public class LeaveRequestDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public LeaveType Type { get; set; }

    public LeaveStatus Status { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Reason { get; set; }

    public DateTime RequestedAt { get; set; }
}
