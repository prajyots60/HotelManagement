namespace ReservationService.DTOs.Reservations;

public class UpdateReservationDto
{
    public int GuestId { get; set; }

    public int RoomId { get; set; }

    public int Adults { get; set; }

    public int Children { get; set; }

    public DateOnly CheckIn { get; set; }

    public DateOnly CheckOut { get; set; }
}
