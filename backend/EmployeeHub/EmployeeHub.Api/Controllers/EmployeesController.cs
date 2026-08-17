using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeHub.Api.Controllers;

[ApiController]
// The directory holds personal data (names, emails, hire dates), so it is manager-and-up only.
// Rank-and-file accounts — including anyone who self-registered — get no access to it.
[Authorize(Policy = "ManagerOrAdmin")]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();

        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
    {
        var result = await _employeeService.CreateEmployeeAsync(dto);

        if (result.Status == ResultStatus.Invalid)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var result = await _employeeService.UpdateEmployeeAsync(id, dto);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Invalid => BadRequest(new { error = result.Error }),
            _ => Ok(result.Value)
        };
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var deleted = await _employeeService.DeleteEmployeeAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
