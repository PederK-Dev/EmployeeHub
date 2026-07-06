namespace EmployeeHub.Api.DTOs;

public class DashboardStatsDto
{
    public int DepartmentCount { get; set; }

    public int EmployeeCount { get; set; }

    public int PositionCount { get; set; }

    public int PendingLeaveCount { get; set; }

    /// <summary>Leave request counts keyed by status name (e.g. "Pending", "Approved").</summary>
    public Dictionary<string, int> LeaveByStatus { get; set; } = new();

    public List<DepartmentHeadcountDto> EmployeesByDepartment { get; set; } = new();

    public List<EmployeeDto> RecentEmployees { get; set; } = new();
}

public class DepartmentHeadcountDto
{
    public string Department { get; set; } = string.Empty;

    public int Count { get; set; }
}
