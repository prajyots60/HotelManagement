using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.Models;

public class Room
{
    public int RoomId { get; set; }

    [Required]
    [MaxLength(20)]
    public string RoomNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RoomType { get; set; } = string.Empty;

    public int Capacity { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Available";

    public ICollection<RoomRate> RoomRates { get; set; } = new List<RoomRate>();
}
