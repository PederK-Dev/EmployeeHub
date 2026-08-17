using System.Linq.Expressions;
using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

/// <summary>
/// Who is asking. Managers and admins see every request; everyone else is limited to the leave
/// belonging to the employee record their account is linked to.
/// </summary>
public record LeaveViewer(int UserId, bool CanViewAll);

public class LeaveRequestService
{
    private readonly EmployeeHubDbContext _context;

    public LeaveRequestService(EmployeeHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveRequestDto>> GetLeaveRequestsAsync(LeaveViewer viewer)
    {
        var query = _context.LeaveRequests.AsQueryable();

        if (!viewer.CanViewAll)
        {
            var employeeId = await LinkedEmployeeIdAsync(viewer.UserId);
            if (employeeId is null)
            {
                // An account that is not linked to an employee record has no leave of its own.
                return [];
            }

            query = query.Where(l => l.EmployeeId == employeeId);
        }

        return await query
            .Select(Project)
            .ToListAsync();
    }

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(int id, LeaveViewer viewer)
    {
        var leaveRequest = await FindProjectedAsync(id);

        if (leaveRequest is null)
        {
            return null;
        }

        if (!viewer.CanViewAll && leaveRequest.EmployeeId != await LinkedEmployeeIdAsync(viewer.UserId))
        {
            // Reported as missing rather than forbidden so ids belonging to other staff
            // cannot be probed.
            return null;
        }

        return leaveRequest;
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(
        CreateLeaveRequestDto dto,
        LeaveViewer viewer)
    {
        var employeeId = dto.EmployeeId;

        if (!viewer.CanViewAll)
        {
            // Without this, any signed-in account could file leave in someone else's name.
            var linkedEmployeeId = await LinkedEmployeeIdAsync(viewer.UserId);
            if (linkedEmployeeId is null)
            {
                return ServiceResult<LeaveRequestDto>.Invalid(
                    "Your account is not linked to an employee record yet, so it cannot request leave.");
            }

            if (employeeId != linkedEmployeeId)
            {
                return ServiceResult<LeaveRequestDto>.Invalid("You can only request leave for yourself.");
            }
        }

        if (dto.EndDate < dto.StartDate)
        {
            return ServiceResult<LeaveRequestDto>.Invalid("End date cannot be earlier than start date.");
        }

        if (!await _context.Employees.AnyAsync(e => e.Id == employeeId))
        {
            return ServiceResult<LeaveRequestDto>.Invalid($"Employee with id {employeeId} does not exist.");
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employeeId,
            Type = dto.Type,
            Status = LeaveStatus.Pending,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            RequestedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        var created = await FindProjectedAsync(leaveRequest.Id);
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

        var updated = await FindProjectedAsync(leaveRequest.Id);
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

    private Task<LeaveRequestDto?> FindProjectedAsync(int id)
    {
        return _context.LeaveRequests
            .Where(l => l.Id == id)
            .Select(Project)
            .FirstOrDefaultAsync();
    }

    /// <summary>The employee record this account belongs to, or null if it is not linked to one.</summary>
    private Task<int?> LinkedEmployeeIdAsync(int userId)
    {
        return _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.EmployeeId)
            .FirstOrDefaultAsync();
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
