using HotelManagementService.Data;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementService.Repositories.Implementations;

public class RoomRepository : IRoomRepository
{
    private readonly HotelDbContext _context;

    public RoomRepository(HotelDbContext context)
    {
        _context = context;
    }

    public Task<List<Room>> GetAllAsync()
    {
        return _context.Rooms
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Room?> GetByIdAsync(int id)
    {
        return _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoomId == id);
    }

    public Task<Room?> GetByRoomNumberAsync(string roomNumber)
    {
        return _context.Rooms
            .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);
    }

    public async Task<Room> CreateAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task<Room?> UpdateAsync(int id, Room room)
    {
        var existing = await _context.Rooms.FindAsync(id);

        if (existing is null)
            return null;

        existing.RoomNumber = room.RoomNumber;
        existing.RoomType = room.RoomType;
        existing.Capacity = room.Capacity;
        existing.Status = room.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room is null)
            return false;

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();
        return true;
    }
}
