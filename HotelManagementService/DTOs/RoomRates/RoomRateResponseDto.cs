namespace HotelManagementService.DTOs.RoomRates;

public class RoomRateResponseDto
{
    public int RateId { get; set; }
    public int RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Day { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public decimal FirstNightPrice { get; set; }
    public decimal ExtensionPrice { get; set; }
}
