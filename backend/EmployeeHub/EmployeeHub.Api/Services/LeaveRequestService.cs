using System.Linq.Expressions;
using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class LeaveRequestService
{
    private readonly EmployeeHubDbContext _context;

    public LeaveRequestService(EmployeeHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveRequestDto>> GetAllLeaveRequestsAsync()
    {
        return await _context.LeaveRequests
            .Select(Project)
            .ToListAsync();
    }

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Where(l => l.Id == id)
            .Select(Project)
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(CreateLeaveRequestDto dto)
    {
        if (dto.EndDate < dto.StartDate)
        {
            return ServiceResult<LeaveRequestDto>.Invalid("End date cannot be earlier than start date.");
        }

        if (!await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId))
        {
            return ServiceResult<LeaveRequestDto>.Invalid($"Employee with id {dto.EmployeeId} does not exist.");
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = dto.EmployeeId,
            Type = dto.Type,
            Status = LeaveStatus.Pending,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            RequestedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        var created = await GetLeaveRequestByIdAsync(leaveRequest.Id);
        return ServiceResult<LeaveRequestDto>.Ok(created!);
    }

    public async Task<ServiceResult<LeaveRequestDto>> UpdateStatusAsync(int id, UpdateLeaveRequestStatusDto dto)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest is null)
        {
            return ServiceResult<LeaveRequestDto>.NotFound();
        }

        leaveRequest.Status = dto.Status;
        await _context.SaveChangesAsync();

        var updated = await GetLeaveRequestByIdAsync(leaveRequest.Id);
        return ServiceResult<LeaveRequestDto>.Ok(updated!);
    }

    public async Task<bool> DeleteLeaveRequestAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest is null)
        {
            return false;
        }

        _context.LeaveRequests.Remove(leaveRequest);
        await _context.SaveChangesAsync();

        return true;
    }

    // Projection expression EF Core can translate into SQL (navigation access becomes JOINs).
    private static readonly Expression<Func<LeaveRequest, LeaveRequestDto>> Project = l =>
        new LeaveRequestDto
        {
            Id = l.Id,
            EmployeeId = l.EmployeeId,
            EmployeeName = l.Employee.FirstName + " " + l.Employee.LastName,
            Type = l.Type,
            Status = l.Status,
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            Reason = l.Reason,
            RequestedAt = l.RequestedAt
        };
}
