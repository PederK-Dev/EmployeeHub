using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class DepartmentService
{
    private readonly EmployeeHubDbContext _context;

    public DepartmentService(EmployeeHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
    {
        return await _context.Departments
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description
            })
            .ToListAsync();
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        return department is null ? null : ToDto(department);
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto)
    {
        var department = new Department
        {
            Name = dto.Name,
            Description = dto.Description
        };

        _context.Departments.Add(department);

        await _context.SaveChangesAsync();

        return ToDto(department);
    }

    public async Task<DepartmentDto?> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department is null)
        {
            return null;
        }

        department.Name = dto.Name;
        department.Description = dto.Description;

        await _context.SaveChangesAsync();

        return ToDto(department);
    }

    public async Task<DeleteResult> DeleteDepartmentAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);

        if (department is null)
        {
            return DeleteResult.NotFound;
        }

        if (await _context.Employees.AnyAsync(e => e.DepartmentId == id))
        {
            return DeleteResult.InUse;
        }

        _context.Departments.Remove(department);

        await _context.SaveChangesAsync();

        return DeleteResult.Deleted;
    }

    private static DepartmentDto ToDto(Department department)
    {
        return new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description
        };
    }
}
