using HotelManagementService.Models;

namespace HotelManagementService.Repositories.Interfaces;

public interface IRoomRateRepository
{
    Task<List<RoomRate>> GetAllAsync();
    Task<RoomRate?> GetByIdAsync(int id);
    Task<bool> RoomExistsAsync(int roomId);
    Task<RoomRate> CreateAsync(RoomRate rate);
    Task<RoomRate?> UpdateAsync(int id, RoomRate rate);
    Task<bool> DeleteAsync(int id);
}
