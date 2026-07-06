namespace EmployeeHub.Api.Models;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly HireDate { get; set; }

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public int PositionId { get; set; }

    public Position Position { get; set; } = null!;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
