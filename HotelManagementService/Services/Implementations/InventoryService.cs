using HotelManagementService.DTOs.Inventory;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;

namespace HotelManagementService.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<InventoryResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToResponse).ToList();
    }

    public async Task<InventoryResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : MapToResponse(item);
    }

    public async Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto)
    {
        var item = new InventoryItem
        {
            ItemName = dto.ItemName.Trim(),
            Quantity = dto.Quantity,
            Unit = dto.Unit.Trim(),
            Description = dto.Description?.Trim()
        };

        var created = await _repository.CreateAsync(item);
        return MapToResponse(created);
    }

    public async Task<InventoryResponseDto?> UpdateAsync(int id, UpdateInventoryDto dto)
    {
        var item = new InventoryItem
        {
            ItemName = dto.ItemName.Trim(),
            Quantity = dto.Quantity,
            Unit = dto.Unit.Trim(),
            Description = dto.Description?.Trim()
        };

        var updated = await _repository.UpdateAsync(id, item);

        return updated is null ? null : MapToResponse(updated);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    private static InventoryResponseDto MapToResponse(InventoryItem item)
    {
        return new InventoryResponseDto
        {
            InventoryId = item.InventoryId,
            ItemName = item.ItemName,
            Quantity = item.Quantity,
            Unit = item.Unit,
            Description = item.Description
        };
    }
}
