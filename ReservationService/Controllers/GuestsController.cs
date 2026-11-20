using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Authorization;
using ReservationService.DTOs.Guests;
using ReservationService.Services.Interfaces;

namespace ReservationService.Controllers;

/// <summary>
/// API Controller exposing CRUD endpoints for managing hotel guests.
/// Inherits from ControllerBase (specialized for Web APIs without Razor View engine overhead).
/// </summary>
[Authorize(Policy = ReservationPolicies.GuestsRead)]
[ApiController] // Enables automatic model validation, ProblemDetails, and parameter binding inferences
[Route("api/[controller]")] // Route token [controller] resolves dynamically to "guests"
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;
    private readonly IValidator<CreateGuestDto> _createValidator;
    private readonly IValidator<UpdateGuestDto> _updateValidator;

    // Dependency Injection: ASP.NET Core supplies the scoped IGuestService and validators
    public GuestsController(
        IGuestService guestService,
        IValidator<CreateGuestDto> createValidator,
        IValidator<UpdateGuestDto> updateValidator)
    {
        _guestService = guestService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// GET: api/guests
    /// Retrieves the list of all registered guests.
    /// ActionResult<T> allows returning either a typed object (wrapped in Ok) or any IActionResult (like NotFound).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestResponseDto>>> GetAll()
    {
        var guests = await _guestService.GetAllAsync();
        return Ok(guests); // Emits HTTP 200 OK with JSON array
    }

    /// <summary>
    /// GET: api/guests/5
    /// Retrieves a single guest by ID.
    /// Route constraint '{id:int}' ensures requests like 'api/guests/abc' fail routing immediately.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GuestResponseDto>> GetById(int id)
    {
        var guest = await _guestService.GetByIdAsync(id);

        if (guest is null)
        {
            return NotFound(new { message = $"Guest with ID {id} was not found." }); // Emits HTTP 404
        }

        return Ok(guest); // Emits HTTP 200 OK
    }

    /// <summary>
    /// POST: api/guests
    /// Registers a new guest.
    /// [FromBody] binds incoming JSON payload from the request stream to CreateGuestDto.
    /// </summary>
    [Authorize(Policy = ReservationPolicies.GuestsManage)]
    [HttpPost]
    public async Task<ActionResult<GuestResponseDto>> Create(
        [FromBody] CreateGuestDto dto)
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

        var createdGuest = await _guestService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdGuest.GuestId },
            createdGuest);
    }

    /// <summary>
    /// PUT: api/guests/5
    /// Updates all fields of an existing guest. Idempotent operation.
    /// </summary>
    [Authorize(Policy = ReservationPolicies.GuestsManage)]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GuestResponseDto>> Update(
        int id,
        [FromBody] UpdateGuestDto dto)
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

        var updatedGuest = await _guestService.UpdateAsync(id, dto);

        if (updatedGuest is null)
        {
            return NotFound(new { message = $"Guest with ID {id} was not found." });
        }

        return Ok(updatedGuest);
    }

    /// <summary>
    /// DELETE: api/guests/5
    /// Removes a guest by ID.
    /// Returns 204 NoContent on success because the resource no longer exists.
    /// </summary>
    [Authorize(Policy = ReservationPolicies.GuestsDelete)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _guestService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"Guest with ID {id} was not found." });
        }

        return NoContent(); // Emits HTTP 204 No Content
    }
}
