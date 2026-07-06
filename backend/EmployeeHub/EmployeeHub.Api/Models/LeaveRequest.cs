namespace EmployeeHub.Api.Models;

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public LeaveType Type { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Reason { get; set; }

    public DateTime RequestedAt { get; set; }
}
