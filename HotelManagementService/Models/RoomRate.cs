using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.Models;

public class RoomRate
{
    [Key]
    public int RateId { get; set; }

    public int RoomId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [Required]
    [MaxLength(15)]
    public string Day { get; set; } = string.Empty;

    public int GuestCount { get; set; }

    public decimal FirstNightPrice { get; set; }

    public decimal ExtensionPrice { get; set; }

    public Room? Room { get; set; }
}
