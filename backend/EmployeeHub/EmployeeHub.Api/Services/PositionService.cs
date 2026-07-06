using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Services;

public class PositionService
{
    private readonly EmployeeHubDbContext _context;

    public PositionService(EmployeeHubDbContext context)
    {
        _context = context;
    }

    public async Task<List<PositionDto>> GetAllPositionsAsync()
    {
        return await _context.Positions
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description
            })
            .ToListAsync();
    }

    public async Task<PositionDto?> GetPositionByIdAsync(int id)
    {
        var position = await _context.Positions.FindAsync(id);

        return position is null ? null : ToDto(position);
    }

    public async Task<PositionDto> CreatePositionAsync(CreatePositionDto dto)
    {
        var position = new Position
        {
            Title = dto.Title,
            Description = dto.Description
        };

        _context.Positions.Add(position);

        await _context.SaveChangesAsync();

        return ToDto(position);
    }

    public async Task<PositionDto?> UpdatePositionAsync(int id, UpdatePositionDto dto)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position is null)
        {
            return null;
        }

        position.Title = dto.Title;
        position.Description = dto.Description;

        await _context.SaveChangesAsync();

        return ToDto(position);
    }

    public async Task<DeleteResult> DeletePositionAsync(int id)
    {
        var position = await _context.Positions.FindAsync(id);

        if (position is null)
        {
            return DeleteResult.NotFound;
        }

        if (await _context.Employees.AnyAsync(e => e.PositionId == id))
        {
            return DeleteResult.InUse;
        }

        _context.Positions.Remove(position);

        await _context.SaveChangesAsync();

        return DeleteResult.Deleted;
    }

    private static PositionDto ToDto(Position position)
    {
        return new PositionDto
        {
            Id = position.Id,
            Title = position.Title,
            Description = position.Description
        };
    }
}
