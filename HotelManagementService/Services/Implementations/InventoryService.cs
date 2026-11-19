using HotelManagementService.DTOs.Inventory;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManagementService.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IInventoryRepository repository,
        ILogger<InventoryService> logger)
    {
        _repository = repository;
        _logger = logger;
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

        _logger.LogInformation(
            "Inventory item created successfully with InventoryId {InventoryId} and ItemName {ItemName}",
            created.InventoryId,
            created.ItemName);

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

        if (updated is null)
        {
            _logger.LogWarning(
                "Failed to update inventory item. Item with InventoryId {InventoryId} was not found",
                id);
            return null;
        }

        _logger.LogInformation(
            "Inventory item {InventoryId} updated successfully to ItemName {ItemName}",
            updated.InventoryId,
            updated.ItemName);

        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning(
                "Failed to delete inventory item. Item with InventoryId {InventoryId} was not found",
                id);
            return false;
        }

        _logger.LogInformation(
            "Inventory item {InventoryId} deleted successfully",
            id);

        return true;
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
