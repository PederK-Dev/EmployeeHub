using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class UserService
{
    private readonly EmployeeHubDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(EmployeeHubDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                EmployeeId = u.EmployeeId
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        return user is null ? null : ToDto(user);
    }

    public async Task<ServiceResult<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
        {
            return ServiceResult<UserDto>.Invalid($"A user with email '{dto.Email}' already exists.");
        }

        if (dto.EmployeeId is int employeeId)
        {
            if (!await _context.Employees.AnyAsync(e => e.Id == employeeId))
            {
                return ServiceResult<UserDto>.Invalid($"Employee with id {employeeId} does not exist.");
            }

            if (await _context.Users.AnyAsync(u => u.EmployeeId == employeeId))
            {
                return ServiceResult<UserDto>.Invalid($"Employee with id {employeeId} is already linked to a user.");
            }
        }

        var user = new User
        {
            Email = dto.Email,
            Role = dto.Role,
            EmployeeId = dto.EmployeeId
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ServiceResult<UserDto>.Ok(ToDto(user));
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            EmployeeId = user.EmployeeId
        };
    }
}
