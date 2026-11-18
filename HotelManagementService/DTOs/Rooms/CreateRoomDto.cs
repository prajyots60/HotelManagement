using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.DTOs.Rooms;

public class CreateRoomDto
{
    [Required(ErrorMessage = "Room number is required.")]
    [MaxLength(20, ErrorMessage = "Room number cannot exceed 20 characters.")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room type is required.")]
    [MaxLength(50, ErrorMessage = "Room type cannot exceed 50 characters.")]
    public string RoomType { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1.")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Available";
}
