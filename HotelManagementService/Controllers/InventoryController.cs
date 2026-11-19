using FluentValidation;
using HotelManagementService.Authorization;
using HotelManagementService.DTOs.Inventory;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize(Policy = HotelPolicies.HotelRead)]
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _service;
    private readonly IValidator<CreateInventoryDto> _createValidator;
    private readonly IValidator<UpdateInventoryDto> _updateValidator;

    public InventoryController(
        IInventoryService service,
        IValidator<CreateInventoryDto> createValidator,
        IValidator<UpdateInventoryDto> updateValidator)
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
        var item = await _service.GetByIdAsync(id);

        if (item is null)
            return NotFound(new { message = "Inventory item not found." });

        return Ok(item);
    }

    [Authorize(Policy = HotelPolicies.InventoryManage)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateInventoryDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var item = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.InventoryId },
            item);
    }

    [Authorize(Policy = HotelPolicies.InventoryManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateInventoryDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var item = await _service.UpdateAsync(id, dto);

        if (item is null)
            return NotFound(new { message = "Inventory item not found." });

        return Ok(item);
    }

    [Authorize(Policy = HotelPolicies.InventoryManage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Inventory item not found." });

        return NoContent();
    }
}
