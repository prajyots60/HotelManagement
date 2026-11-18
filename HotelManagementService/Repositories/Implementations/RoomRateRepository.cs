using HotelManagementService.Data;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementService.Repositories.Implementations;

public class RoomRateRepository : IRoomRateRepository
{
    private readonly HotelDbContext _context;

    public RoomRateRepository(HotelDbContext context)
    {
        _context = context;
    }

    public Task<List<RoomRate>> GetAllAsync()
    {
        return _context.RoomRates
            .AsNoTracking()
            .Include(r => r.Room)
            .ToListAsync();
    }

    public Task<RoomRate?> GetByIdAsync(int id)
    {
        return _context.RoomRates
            .AsNoTracking()
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.RateId == id);
    }

    public Task<bool> RoomExistsAsync(int roomId)
    {
        return _context.Rooms.AnyAsync(r => r.RoomId == roomId);
    }

    public async Task<RoomRate> CreateAsync(RoomRate rate)
    {
        _context.RoomRates.Add(rate);
        await _context.SaveChangesAsync();

        return await _context.RoomRates
            .AsNoTracking()
            .Include(r => r.Room)
            .FirstAsync(r => r.RateId == rate.RateId);
    }

    public async Task<RoomRate?> UpdateAsync(int id, RoomRate rate)
    {
        var existing = await _context.RoomRates.FindAsync(id);

        if (existing is null)
            return null;

        existing.RoomId = rate.RoomId;
        existing.StartDate = rate.StartDate;
        existing.EndDate = rate.EndDate;
        existing.Day = rate.Day;
        existing.GuestCount = rate.GuestCount;
        existing.FirstNightPrice = rate.FirstNightPrice;
        existing.ExtensionPrice = rate.ExtensionPrice;

        await _context.SaveChangesAsync();

        return await _context.RoomRates
            .AsNoTracking()
            .Include(r => r.Room)
            .FirstAsync(r => r.RateId == id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var rate = await _context.RoomRates.FindAsync(id);

        if (rate is null)
            return false;

        _context.RoomRates.Remove(rate);
        await _context.SaveChangesAsync();

        return true;
    }
}
