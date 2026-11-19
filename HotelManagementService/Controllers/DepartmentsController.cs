using FluentValidation;
using HotelManagementService.Authorization;
using HotelManagementService.DTOs.Departments;
using HotelManagementService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementService.Controllers;

[Authorize(Policy = HotelPolicies.HotelRead)]
[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;
    private readonly IValidator<CreateDepartmentDto> _createValidator;
    private readonly IValidator<UpdateDepartmentDto> _updateValidator;

    public DepartmentsController(
        IDepartmentService service,
        IValidator<CreateDepartmentDto> createValidator,
        IValidator<UpdateDepartmentDto> updateValidator)
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
        var department = await _service.GetByIdAsync(id);

        if (department is null)
            return NotFound(new { message = "Department not found." });

        return Ok(department);
    }

    [Authorize(Policy = HotelPolicies.DepartmentsManage)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDepartmentDto dto)
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

        var department = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = department.DepartmentId },
            department);
    }

    [Authorize(Policy = HotelPolicies.DepartmentsManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, UpdateDepartmentDto dto)
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

        var department = await _service.UpdateAsync(id, dto);

        if (department is null)
            return NotFound(new { message = "Department not found." });

        return Ok(department);
    }

    [Authorize(Policy = HotelPolicies.DepartmentsManage)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Department not found." });

        return NoContent();
    }
}
