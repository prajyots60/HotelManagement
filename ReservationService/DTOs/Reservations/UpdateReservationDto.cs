using System.ComponentModel.DataAnnotations;

namespace ReservationService.DTOs.Reservations;

/// <summary>
/// Data Transfer Object for updating an existing reservation's dates, room, or occupants.
/// Note that Status is updated via its own dedicated endpoint (UpdateReservationStatusDto)
/// to ensure status transition workflows (e.g. Pending -> Confirmed -> CheckedIn) remain controlled.
/// </summary>
public class UpdateReservationDto
{
    [Range(1, int.MaxValue, ErrorMessage = "GuestId must be greater than zero.")]
    public int GuestId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "RoomId must be greater than zero.")]
    public int RoomId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "At least one adult is required.")]
    public int Adults { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Children cannot be negative.")]
    public int Children { get; set; }

    [Required(ErrorMessage = "Check-in date is required.")]
    public DateOnly CheckIn { get; set; }

    [Required(ErrorMessage = "Check-out date is required.")]
    public DateOnly CheckOut { get; set; }
}
