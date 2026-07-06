using System.Linq.Expressions;
using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class DashboardService
{
    private readonly EmployeeHubDbContext _context;

    public DashboardService(EmployeeHubDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var leaveByStatus = await _context.LeaveRequests
            .GroupBy(l => l.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var employeesByDepartment = await _context.Departments
            .Select(d => new DepartmentHeadcountDto
            {
                Department = d.Name,
                Count = _context.Employees.Count(e => e.DepartmentId == d.Id)
            })
            .ToListAsync();

        var recentEmployees = await _context.Employees
            .OrderByDescending(e => e.Id)
            .Take(5)
            .Select(Project)
            .ToListAsync();

        return new DashboardStatsDto
        {
            DepartmentCount = await _context.Departments.CountAsync(),
            EmployeeCount = await _context.Employees.CountAsync(),
            PositionCount = await _context.Positions.CountAsync(),
            PendingLeaveCount = leaveByStatus
                .Where(x => x.Status == LeaveStatus.Pending)
                .Sum(x => x.Count),
            LeaveByStatus = leaveByStatus.ToDictionary(x => x.Status.ToString(), x => x.Count),
            EmployeesByDepartment = employeesByDepartment,
            RecentEmployees = recentEmployees,
        };
    }

    // Projection expression EF Core can translate into SQL (navigation access becomes JOINs).
    private static readonly Expression<Func<Employee, EmployeeDto>> Project = e =>
        new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            HireDate = e.HireDate,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department.Name,
            PositionId = e.PositionId,
            PositionTitle = e.Position.Title,
        };
}
