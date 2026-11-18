using HotelManagementService.DTOs.Rooms;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;

namespace HotelManagementService.Services.Implementations;

public class RoomService : IRoomService
{
    private static readonly string[] AllowedStatuses =
    [
        "Available",
        "Occupied",
        "Maintenance"
    ];

    private readonly IRoomRepository _repository;

    public RoomService(IRoomRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RoomResponseDto>> GetAllAsync()
    {
        var rooms = await _repository.GetAllAsync();
        return rooms.Select(MapToResponse).ToList();
    }

    public async Task<RoomResponseDto?> GetByIdAsync(int id)
    {
        var room = await _repository.GetByIdAsync(id);
        return room is null ? null : MapToResponse(room);
    }

    public async Task<(RoomResponseDto? Data, string? Error)> CreateAsync(CreateRoomDto dto)
    {
        var roomNumber = dto.RoomNumber.Trim();
        var status = NormalizeStatus(dto.Status);

        if (status is null)
            return (null, "Invalid room status. Allowed values: Available, Occupied, Maintenance.");

        if (await _repository.GetByRoomNumberAsync(roomNumber) is not null)
            return (null, "Room number already exists.");

        var room = new Room
        {
            RoomNumber = roomNumber,
            RoomType = dto.RoomType.Trim(),
            Capacity = dto.Capacity,
            Status = status
        };

        var created = await _repository.CreateAsync(room);
        return (MapToResponse(created), null);
    }

    public async Task<(RoomResponseDto? Data, string? Error)> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return (null, null);

        var roomNumber = dto.RoomNumber.Trim();
        var status = NormalizeStatus(dto.Status);

        if (status is null)
            return (null, "Invalid room status. Allowed values: Available, Occupied, Maintenance.");

        var duplicate = await _repository.GetByRoomNumberAsync(roomNumber);

        if (duplicate is not null && duplicate.RoomId != id)
            return (null, "Room number already exists.");

        var room = new Room
        {
            RoomNumber = roomNumber,
            RoomType = dto.RoomType.Trim(),
            Capacity = dto.Capacity,
            Status = status
        };

        var updated = await _repository.UpdateAsync(id, room);

        return (updated is null ? null : MapToResponse(updated), null);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    private static string? NormalizeStatus(string status)
    {
        return AllowedStatuses.FirstOrDefault(
            s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static RoomResponseDto MapToResponse(Room room)
    {
        return new RoomResponseDto
        {
            RoomId = room.RoomId,
            RoomNumber = room.RoomNumber,
            RoomType = room.RoomType,
            Capacity = room.Capacity,
            Status = room.Status
        };
    }
}
