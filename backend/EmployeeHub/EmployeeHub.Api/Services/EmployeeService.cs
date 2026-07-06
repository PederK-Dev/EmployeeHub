using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class EmployeeService
{
    private readonly EmployeeHubDbContext _context;

    public EmployeeService(EmployeeHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Select(Project)
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .Where(e => e.Id == id)
            .Select(Project)
            .FirstOrDefaultAsync();
    }

    // Projection expression EF Core can translate into SQL (navigation access becomes JOINs).
    private static readonly System.Linq.Expressions.Expression<Func<Employee, EmployeeDto>> Project = e =>
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
            PositionTitle = e.Position.Title
        };

    public async Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        var validationError = await ValidateReferencesAsync(dto.DepartmentId, dto.PositionId);
        if (validationError is not null)
        {
            return ServiceResult<EmployeeDto>.Invalid(validationError);
        }

        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email))
        {
            return ServiceResult<EmployeeDto>.Invalid($"An employee with email '{dto.Email}' already exists.");
        }

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            HireDate = dto.HireDate,
            DepartmentId = dto.DepartmentId,
            PositionId = dto.PositionId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Reload with navigation names for the response.
        var created = await GetEmployeeByIdAsync(employee.Id);
        return ServiceResult<EmployeeDto>.Ok(created!);
    }

    public async Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return ServiceResult<EmployeeDto>.NotFound();
        }

        var validationError = await ValidateReferencesAsync(dto.DepartmentId, dto.PositionId);
        if (validationError is not null)
        {
            return ServiceResult<EmployeeDto>.Invalid(validationError);
        }

        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id))
        {
            return ServiceResult<EmployeeDto>.Invalid($"An employee with email '{dto.Email}' already exists.");
        }

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.HireDate = dto.HireDate;
        employee.DepartmentId = dto.DepartmentId;
        employee.PositionId = dto.PositionId;

        await _context.SaveChangesAsync();

        var updated = await GetEmployeeByIdAsync(employee.Id);
        return ServiceResult<EmployeeDto>.Ok(updated!);
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return false;
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<string?> ValidateReferencesAsync(int departmentId, int positionId)
    {
        if (!await _context.Departments.AnyAsync(d => d.Id == departmentId))
        {
            return $"Department with id {departmentId} does not exist.";
        }

        if (!await _context.Positions.AnyAsync(p => p.Id == positionId))
        {
            return $"Position with id {positionId} does not exist.";
        }

        return null;
    }
}
