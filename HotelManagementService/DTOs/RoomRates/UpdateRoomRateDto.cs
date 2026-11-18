using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.DTOs.RoomRates;

public class UpdateRoomRateDto
{
    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Required, MaxLength(15)]
    public string Day { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Guest count must be greater than zero.")]
    public int GuestCount { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "First night price must be greater than zero.")]
    public decimal FirstNightPrice { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Extension price must be greater than zero.")]
    public decimal ExtensionPrice { get; set; }
}
