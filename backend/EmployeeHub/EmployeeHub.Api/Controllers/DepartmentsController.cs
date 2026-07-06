using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _departmentService;

    public DepartmentsController(DepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();

        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);

        if (department is null)
        {
            return NotFound();
        }

        return Ok(department);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDepartment(CreateDepartmentDto dto)
    {
        var department = await _departmentService.CreateDepartmentAsync(dto);

        return CreatedAtAction(
            nameof(GetDepartment),
            new { id = department.Id },
            department);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto dto)
    {
        var department = await _departmentService.UpdateDepartmentAsync(id, dto);

        if (department is null)
        {
            return NotFound();
        }

        return Ok(department);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var result = await _departmentService.DeleteDepartmentAsync(id);

        return result switch
        {
            DeleteResult.NotFound => NotFound(),
            DeleteResult.InUse => Conflict(new { error = "Cannot delete a department that still has employees." }),
            _ => NoContent()
        };
    }
}
