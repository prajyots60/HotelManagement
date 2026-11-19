using FluentValidation;
using HotelManagementService.Authorization;
using HotelManagementService.DTOs.RoomRates;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize(Policy = HotelPolicies.HotelRead)]
[ApiController]
[Route("api/rates")]
public class RoomRatesController : ControllerBase
{
    private readonly IRoomRateService _service;
    private readonly IValidator<CreateRoomRateDto> _createValidator;
    private readonly IValidator<UpdateRoomRateDto> _updateValidator;

    public RoomRatesController(
        IRoomRateService service,
        IValidator<CreateRoomRateDto> createValidator,
        IValidator<UpdateRoomRateDto> updateValidator)
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
        var rate = await _service.GetByIdAsync(id);

        if (rate is null)
            return NotFound(new { message = "Room rate not found." });

        return Ok(rate);
    }

    [Authorize(Policy = HotelPolicies.RoomRatesManage)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomRateDto dto)
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
            new { id = data.RateId },
            data);
    }

    [Authorize(Policy = HotelPolicies.RoomRatesManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRoomRateDto dto)
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
            return NotFound(new { message = "Room rate not found." });

        return Ok(data);
    }

    [Authorize(Policy = HotelPolicies.RoomRatesManage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Room rate not found." });

        return NoContent();
    }
}
