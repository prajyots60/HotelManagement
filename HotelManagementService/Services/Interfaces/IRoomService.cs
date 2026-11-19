using HotelManagementService.DTOs.Rooms;

namespace HotelManagementService.Services.Interfaces;

public interface IRoomService
{
    Task<List<RoomResponseDto>> GetAllAsync();
    Task<RoomResponseDto?> GetByIdAsync(int id);
    Task<RoomResponseDto> CreateAsync(CreateRoomDto dto);
    Task<RoomResponseDto?> UpdateAsync(int id, UpdateRoomDto dto);
    Task<bool> DeleteAsync(int id);
}
