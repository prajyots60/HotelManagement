using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.DTOs.Reservations;
using ReservationService.Services.Interfaces;

namespace ReservationService.Controllers;

/// <summary>
/// Manages hotel reservation lifecycles.
/// Secured by [Authorize] requiring a valid JWT Bearer token.
/// </summary>
[Authorize] // Enforces JWT authentication across all endpoints in this controller
[ApiController] // Activates automated model state validation and ProblemDetails
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET: api/reservations
    /// Lists all reservations in the system.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    /// <summary>
    /// GET: api/reservations/{id}
    /// Retrieves a single reservation by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await _service.GetByIdAsync(id);

        if (reservation is null)
            return NotFound(new { message = "Reservation not found." });

        return Ok(reservation);
    }

    /// <summary>
    /// POST: api/reservations
    /// Creates a new reservation after validating guest, room, stay dates, and scheduling conflicts.
    /// Translates domain exceptions into standard HTTP status codes:
    /// - KeyNotFoundException -> 404 (Guest or Room does not exist)
    /// - InvalidOperationException -> 409 (Overlap conflict or capacity exceeded)
    /// - ArgumentException -> 400 (CheckIn >= CheckOut)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        try
        {
            var reservation = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = reservation.ReservationId },
                reservation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/reservations/{id}
    /// Modifies reservation schedule or guest counts, checking for room availability conflicts.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateReservationDto dto)
    {
        try
        {
            var reservation = await _service.UpdateAsync(id, dto);

            if (reservation is null)
                return NotFound(new { message = "Reservation not found." });

            return Ok(reservation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// PUT: api/reservations/{id}/status
    /// Dedicated action for reservation lifecycle transitions (Pending, Confirmed, CheckedIn, CheckedOut, Cancelled).
    /// </summary>
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateReservationStatusDto dto)
    {
        try
        {
            var reservation = await _service.UpdateStatusAsync(id, dto);

            if (reservation is null)
                return NotFound(new { message = "Reservation not found." });

            return Ok(reservation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
