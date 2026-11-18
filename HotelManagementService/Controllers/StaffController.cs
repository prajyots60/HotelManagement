using HotelManagementService.DTOs.Staff;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _service;

    public StaffController(IStaffService service)
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
        var staff = await _service.GetByIdAsync(id);

        if (staff is null)
            return NotFound(new { message = "Staff member not found." });

        return Ok(staff);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStaffDto dto)
    {
        var (data, error) = await _service.CreateAsync(dto);

        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = data!.StaffId },
            data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateStaffDto dto)
    {
        var (data, error) = await _service.UpdateAsync(id, dto);

        if (error is not null)
            return BadRequest(new { message = error });

        if (data is null)
            return NotFound(new { message = "Staff member not found." });

        return Ok(data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Staff member not found." });

        return NoContent();
    }
}
