namespace HotelManagementService.DTOs.Rooms;

public class CreateRoomDto
{
    public string RoomNumber { get; set; } = string.Empty;

    public string RoomType { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = "Available";
}
