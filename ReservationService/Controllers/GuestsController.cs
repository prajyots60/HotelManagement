using Microsoft.AspNetCore.Mvc;
using ReservationService.DTOs.Guests;
using ReservationService.Services.Interfaces;

namespace ReservationService.Controllers;

/// <summary>
/// API Controller exposing CRUD endpoints for managing hotel guests.
/// Inherits from ControllerBase (specialized for Web APIs without Razor View engine overhead).
/// </summary>
[ApiController] // Enables automatic model validation, ProblemDetails, and parameter binding inferences
[Route("api/[controller]")] // Route token [controller] resolves dynamically to "guests"
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    // Dependency Injection: ASP.NET Core supplies the scoped IGuestService implementation
    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
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
    [HttpPost]
    public async Task<ActionResult<GuestResponseDto>> Create(
        [FromBody] CreateGuestDto dto)
    {
        try
        {
            var createdGuest = await _guestService.CreateAsync(dto);

            // CreatedAtAction emits HTTP 201 Created and sets the 'Location' response header:
            // Location: http://localhost:7003/api/guests/{id}
            return CreatedAtAction(
                nameof(GetById),
                new { id = createdGuest.GuestId },
                createdGuest);
        }
        catch (InvalidOperationException ex)
        {
            // Translates duplicate MemberCode business exception into RFC-standard HTTP 409 Conflict
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/guests/5
    /// Updates all fields of an existing guest. Idempotent operation.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GuestResponseDto>> Update(
        int id,
        [FromBody] UpdateGuestDto dto)
    {
        try
        {
            var updatedGuest = await _guestService.UpdateAsync(id, dto);

            if (updatedGuest is null)
            {
                return NotFound(new { message = $"Guest with ID {id} was not found." });
            }

            return Ok(updatedGuest); // Emits HTTP 200 OK
        }
        catch (InvalidOperationException ex)
        {
            // Rejects update if client tries to take another guest's MemberCode
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// DELETE: api/guests/5
    /// Removes a guest by ID.
    /// Returns 204 NoContent on success because the resource no longer exists.
    /// </summary>
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
