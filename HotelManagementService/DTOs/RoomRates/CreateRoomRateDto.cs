namespace HotelManagementService.DTOs.RoomRates;

public class CreateRoomRateDto
{
    public int RoomId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Day { get; set; } = string.Empty;

    public int GuestCount { get; set; }

    public decimal FirstNightPrice { get; set; }

    public decimal ExtensionPrice { get; set; }
}
