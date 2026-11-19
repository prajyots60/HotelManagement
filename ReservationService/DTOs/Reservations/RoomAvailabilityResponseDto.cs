namespace ReservationService.DTOs.Reservations;

/// <summary>
/// Data Transfer Object representing room details in an availability query.
/// Decouples the ReservationService client from HotelManagementService's internal Room entity.
/// </summary>
public class RoomAvailabilityResponseDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}
