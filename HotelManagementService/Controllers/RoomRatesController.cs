using HotelManagementService.DTOs.RoomRates;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize]
[ApiController]
[Route("api/rates")]
public class RoomRatesController : ControllerBase
{
    private readonly IRoomRateService _service;

    public RoomRatesController(IRoomRateService service)
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
        var rate = await _service.GetByIdAsync(id);

        if (rate is null)
            return NotFound(new { message = "Room rate not found." });

        return Ok(rate);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomRateDto dto)
    {
        var (data, error) = await _service.CreateAsync(dto);

        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = data!.RateId },
            data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomRateDto dto)
    {
        var (data, error) = await _service.UpdateAsync(id, dto);

        if (error is not null)
            return BadRequest(new { message = error });

        if (data is null)
            return NotFound(new { message = "Room rate not found." });

        return Ok(data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Room rate not found." });

        return NoContent();
    }
}
