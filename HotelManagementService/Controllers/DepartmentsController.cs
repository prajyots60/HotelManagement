using HotelManagementService.DTOs.Departments;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize]
[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var department = await _service.GetByIdAsync(id);

        if (department is null)
            return NotFound(new { message = "Department not found." });

        return Ok(department);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
    {
        var department = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = department.DepartmentId },
            department);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateDepartmentDto dto)
    {
        var department = await _service.UpdateAsync(id, dto);

        if (department is null)
            return NotFound(new { message = "Department not found." });

        return Ok(department);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Department not found." });

        return NoContent();
    }
}
