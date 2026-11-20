using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Authorization;
using ReservationService.DTOs.Reservations;
using ReservationService.Services.Interfaces;

namespace ReservationService.Controllers;

/// <summary>
/// Manages hotel reservation lifecycles.
/// Secured by [Authorize] requiring a valid JWT Bearer token.
/// </summary>
[Authorize(Policy = ReservationPolicies.ReservationsRead)]
[ApiController] // Activates automated model state validation and ProblemDetails
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;
    private readonly IValidator<CreateReservationDto> _createValidator;
    private readonly IValidator<UpdateReservationDto> _updateValidator;
    private readonly IValidator<UpdateReservationStatusDto> _statusValidator;

    public ReservationsController(
        IReservationService service,
        IValidator<CreateReservationDto> createValidator,
        IValidator<UpdateReservationDto> updateValidator,
        IValidator<UpdateReservationStatusDto> statusValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _statusValidator = statusValidator;
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
    /// </summary>
    [Authorize(Policy = ReservationPolicies.ReservationsManage)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
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

        var reservation = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = reservation.ReservationId },
            reservation);
    }

    /// <summary>
    /// PUT: api/reservations/{id}
    /// Modifies reservation schedule or guest counts, checking for room availability conflicts.
    /// </summary>
    [Authorize(Policy = ReservationPolicies.ReservationsManage)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateReservationDto dto)
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

        var reservation = await _service.UpdateAsync(id, dto);

        if (reservation is null)
            return NotFound(new { message = "Reservation not found." });

        return Ok(reservation);
    }

    /// <summary>
    /// PUT: api/reservations/{id}/status
    /// Dedicated action for reservation lifecycle transitions (Pending, Confirmed, CheckedIn, CheckedOut, Cancelled).
    /// </summary>
    [Authorize(Policy = ReservationPolicies.ReservationsManage)]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateReservationStatusDto dto)
    {
        var validationResult = await _statusValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }
            return ValidationProblem(ModelState);
        }

        var reservation = await _service.UpdateStatusAsync(id, dto);

        if (reservation is null)
            return NotFound(new { message = "Reservation not found." });

        return Ok(reservation);
    }
}
