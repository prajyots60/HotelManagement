using HotelManagementService.DTOs.Rooms;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize]
[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _service;

    public RoomsController(IRoomService service)
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
        var room = await _service.GetByIdAsync(id);

        if (room is null)
            return NotFound(new { message = "Room not found." });

        return Ok(room);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomDto dto)
    {
        var (data, error) = await _service.CreateAsync(dto);

        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = data!.RoomId },
            data);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomDto dto)
    {
        var (data, error) = await _service.UpdateAsync(id, dto);

        if (error is not null)
            return BadRequest(new { message = error });

        if (data is null)
            return NotFound(new { message = "Room not found." });

        return Ok(data);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Room not found." });

        return NoContent();
    }
}
