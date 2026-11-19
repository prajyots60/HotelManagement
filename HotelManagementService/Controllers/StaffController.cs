using FluentValidation;
using HotelManagementService.Authorization;
using HotelManagementService.DTOs.Staff;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize(Policy = HotelPolicies.HotelRead)]
[ApiController]
[Route("api/staff")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _service;
    private readonly IValidator<CreateStaffDto> _createValidator;
    private readonly IValidator<UpdateStaffDto> _updateValidator;

    public StaffController(
        IStaffService service,
        IValidator<CreateStaffDto> createValidator,
        IValidator<UpdateStaffDto> updateValidator)
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
        var staff = await _service.GetByIdAsync(id);

        if (staff is null)
            return NotFound(new { message = "Staff member not found." });

        return Ok(staff);
    }

    [Authorize(Policy = HotelPolicies.StaffManage)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateStaffDto dto)
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
            new { id = data.StaffId },
            data);
    }

    [Authorize(Policy = HotelPolicies.StaffManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateStaffDto dto)
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
            return NotFound(new { message = "Staff member not found." });

        return Ok(data);
    }

    [Authorize(Policy = HotelPolicies.StaffManage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Staff member not found." });

        return NoContent();
    }
}
