using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Authorization;
using ReservationService.Services.Interfaces;

namespace ReservationService.Controllers;

/// <summary>
/// Exposes room query endpoints owned by the reservation bounded context.
/// Even though physical room data resides in HotelManagementService,
/// "Availability" requires checking active reservation schedules in ReservationDB.
/// </summary>
[Authorize(Policy = ReservationPolicies.RoomAvailabilityRead)]
[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IReservationService _service;

    public RoomsController(IReservationService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET: api/rooms/availability?checkIn=2026-12-01&checkOut=2026-12-05&guests=2
    /// Searches available rooms by cross-referencing room capacity with active booking intervals.
    /// [FromQuery] binds URL query string parameters to method arguments.
    /// </summary>
    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int guests)
    {
        var rooms = await _service.GetAvailableRoomsAsync(
            checkIn,
            checkOut,
            guests);

        return Ok(rooms);
    }
}
