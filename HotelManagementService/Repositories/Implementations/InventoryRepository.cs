using HotelManagementService.Data;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementService.Repositories.Implementations;

public class InventoryRepository : IInventoryRepository
{
    private readonly HotelDbContext _context;

    public InventoryRepository(HotelDbContext context)
    {
        _context = context;
    }

    public Task<List<InventoryItem>> GetAllAsync()
    {
        return _context.Inventory
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<InventoryItem?> GetByIdAsync(int id)
    {
        return _context.Inventory
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InventoryId == id);
    }

    public async Task<InventoryItem> CreateAsync(InventoryItem item)
    {
        _context.Inventory.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem?> UpdateAsync(int id, InventoryItem item)
    {
        var existing = await _context.Inventory.FindAsync(id);

        if (existing is null)
            return null;

        existing.ItemName = item.ItemName;
        existing.Quantity = item.Quantity;
        existing.Unit = item.Unit;
        existing.Description = item.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.Inventory.FindAsync(id);

        if (item is null)
            return false;

        _context.Inventory.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}
