using HotelManagementService.Models;

namespace HotelManagementService.Repositories.Interfaces;

public interface IInventoryRepository
{
    Task<List<InventoryItem>> GetAllAsync();
    Task<InventoryItem?> GetByIdAsync(int id);
    Task<InventoryItem> CreateAsync(InventoryItem item);
    Task<InventoryItem?> UpdateAsync(int id, InventoryItem item);
    Task<bool> DeleteAsync(int id);
}
