using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Extensions;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/leave-requests")]
public class LeaveRequestsController : ControllerBase
{
    private readonly LeaveRequestService _leaveRequestService;

    public LeaveRequestsController(LeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LeaveRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveRequests()
    {
        var viewer = CurrentViewer();
        if (viewer is null)
        {
            return Unauthorized();
        }

        var leaveRequests = await _leaveRequestService.GetLeaveRequestsAsync(viewer);

        return Ok(leaveRequests);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLeaveRequest(int id)
    {
        var viewer = CurrentViewer();
        if (viewer is null)
        {
            return Unauthorized();
        }

        var leaveRequest = await _leaveRequestService.GetLeaveRequestByIdAsync(id, viewer);

        if (leaveRequest is null)
        {
            return NotFound();
        }

        return Ok(leaveRequest);
    }

    [HttpPost]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLeaveRequest(CreateLeaveRequestDto dto)
    {
        var viewer = CurrentViewer();
        if (viewer is null)
        {
            return Unauthorized();
        }

        var result = await _leaveRequestService.CreateLeaveRequestAsync(dto, viewer);

        if (result.Status == ResultStatus.Invalid)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetLeaveRequest),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:int}/status")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(LeaveRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(int id, UpdateLeaveRequestStatusDto dto)
    {
        var result = await _leaveRequestService.UpdateStatusAsync(id, dto);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(new { error = result.Error }),
            _ => Ok(result.Value)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLeaveRequest(int id)
    {
        var deleted = await _leaveRequestService.DeleteLeaveRequestAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private LeaveViewer? CurrentViewer()
    {
        var userId = User.GetUserId();

        return userId is null ? null : new LeaveViewer(userId.Value, User.CanViewAllStaff());
    }
}
