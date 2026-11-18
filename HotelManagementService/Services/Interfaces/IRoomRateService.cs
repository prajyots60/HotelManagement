using HotelManagementService.DTOs.RoomRates;

namespace HotelManagementService.Services.Interfaces;

public interface IRoomRateService
{
    Task<List<RoomRateResponseDto>> GetAllAsync();
    Task<RoomRateResponseDto?> GetByIdAsync(int id);
    Task<(RoomRateResponseDto? Data, string? Error)> CreateAsync(CreateRoomRateDto dto);
    Task<(RoomRateResponseDto? Data, string? Error)> UpdateAsync(int id, UpdateRoomRateDto dto);
    Task<bool> DeleteAsync(int id);
}
