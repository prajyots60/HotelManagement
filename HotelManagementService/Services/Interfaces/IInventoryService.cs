using HotelManagementService.DTOs.Inventory;

namespace HotelManagementService.Services.Interfaces;

public interface IInventoryService
{
    Task<List<InventoryResponseDto>> GetAllAsync();
    Task<InventoryResponseDto?> GetByIdAsync(int id);
    Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto);
    Task<InventoryResponseDto?> UpdateAsync(int id, UpdateInventoryDto dto);
    Task<bool> DeleteAsync(int id);
}
