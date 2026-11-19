using System.ComponentModel.DataAnnotations;

namespace ReservationService.DTOs.Reservations;

/// <summary>
/// Data Transfer Object for creating a new hotel reservation.
/// Notice that ReservationCode, Nights, and Status are intentionally absent:
/// they are managed exclusively by the backend to prevent client tampering.
/// </summary>
public class CreateReservationDto
{
    // GuestId must refer to an existing record in [ReservationDB].[dbo].[Guests]
    [Range(1, int.MaxValue, ErrorMessage = "GuestId must be greater than zero.")]
    public int GuestId { get; set; }

    // RoomId is a logical reference to a Room in HotelManagementService (HotelDB)
    [Range(1, int.MaxValue, ErrorMessage = "RoomId must be greater than zero.")]
    public int RoomId { get; set; }

    // Business rule: every booking must have at least one adult
    [Range(1, int.MaxValue, ErrorMessage = "At least one adult is required.")]
    public int Adults { get; set; }

    // Children count cannot be negative, but can be 0
    [Range(0, int.MaxValue, ErrorMessage = "Children cannot be negative.")]
    public int Children { get; set; }

    // DateOnly (.NET 6+) serializes to 'YYYY-MM-DD' without time or timezone distortion
    [Required(ErrorMessage = "Check-in date is required.")]
    public DateOnly CheckIn { get; set; }

    [Required(ErrorMessage = "Check-out date is required.")]
    public DateOnly CheckOut { get; set; }
}
