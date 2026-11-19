using FluentValidation;
using HotelManagementService.Authorization;
using HotelManagementService.DTOs.Rooms;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize(Policy = HotelPolicies.HotelRead)]
[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _service;
    private readonly IValidator<CreateRoomDto> _createValidator;
    private readonly IValidator<UpdateRoomDto> _updateValidator;

    public RoomsController(
        IRoomService service,
        IValidator<CreateRoomDto> createValidator,
        IValidator<UpdateRoomDto> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
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

    [Authorize(Policy = HotelPolicies.RoomsManage)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var data = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = data.RoomId },
            data);
    }

    [Authorize(Policy = HotelPolicies.RoomsManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var data = await _service.UpdateAsync(id, dto);

        if (data is null)
            return NotFound(new { message = "Room not found." });

        return Ok(data);
    }

    [Authorize(Policy = HotelPolicies.RoomsManage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Room not found." });

        return NoContent();
    }
}
