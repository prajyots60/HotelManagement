using HotelManagementService.DTOs.RoomRates;

namespace HotelManagementService.Services.Interfaces;

public interface IRoomRateService
{
    Task<List<RoomRateResponseDto>> GetAllAsync();
    Task<RoomRateResponseDto?> GetByIdAsync(int id);
    Task<RoomRateResponseDto> CreateAsync(CreateRoomRateDto dto);
    Task<RoomRateResponseDto?> UpdateAsync(int id, UpdateRoomRateDto dto);
    Task<bool> DeleteAsync(int id);
}
