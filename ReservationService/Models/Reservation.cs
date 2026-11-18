using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models;

public class Reservation
{
    public int ReservationId { get; set; }

    [Required]
    [MaxLength(30)]
    public string ReservationCode { get; set; } = string.Empty;

    public int GuestId { get; set; }

    // Logical reference to a room in HotelManagementService.
    public int RoomId { get; set; }

    public int Adults { get; set; }

    public int Children { get; set; }

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }

    public int Nights { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public Guest? Guest { get; set; }
}
