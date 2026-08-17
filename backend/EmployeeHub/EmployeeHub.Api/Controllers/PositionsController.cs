using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly PositionService _positionService;

    public PositionsController(PositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PositionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions()
    {
        var positions = await _positionService.GetAllPositionsAsync();

        return Ok(positions);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPosition(int id)
    {
        var position = await _positionService.GetPositionByIdAsync(id);

        if (position is null)
        {
            return NotFound();
        }

        return Ok(position);
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePosition(CreatePositionDto dto)
    {
        var position = await _positionService.CreatePositionAsync(dto);

        return CreatedAtAction(
            nameof(GetPosition),
            new { id = position.Id },
            position);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(PositionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePosition(int id, UpdatePositionDto dto)
    {
        var position = await _positionService.UpdatePositionAsync(id, dto);

        if (position is null)
        {
            return NotFound();
        }

        return Ok(position);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeletePosition(int id)
    {
        var result = await _positionService.DeletePositionAsync(id);

        return result switch
        {
            DeleteResult.NotFound => NotFound(),
            DeleteResult.InUse => Conflict(new { error = "Cannot delete a position that still has employees." }),
            _ => NoContent()
        };
    }
}
