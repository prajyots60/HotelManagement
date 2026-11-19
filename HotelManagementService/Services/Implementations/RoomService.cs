using HotelManagementService.DTOs.Rooms;
using HotelManagementService.Exceptions;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;
using Microsoft.Extensions.Logging;

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
    private readonly ILogger<RoomService> _logger;

    public RoomService(
        IRoomRepository repository,
        ILogger<RoomService> logger)
    {
        _repository = repository;
        _logger = logger;
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

    public async Task<RoomResponseDto> CreateAsync(CreateRoomDto dto)
    {
        var roomNumber = dto.RoomNumber.Trim();
        var status = NormalizeStatus(dto.Status) ?? "Available";

        if (await _repository.GetByRoomNumberAsync(roomNumber) is not null)
        {
            _logger.LogWarning(
                "Failed to create room. RoomNumber {RoomNumber} already exists",
                roomNumber);
            throw new ConflictException("Room number already exists.");
        }

        var room = new Room
        {
            RoomNumber = roomNumber,
            RoomType = dto.RoomType.Trim(),
            Capacity = dto.Capacity,
            Status = status
        };

        var created = await _repository.CreateAsync(room);

        _logger.LogInformation(
            "Room created successfully with RoomId {RoomId}, RoomNumber {RoomNumber}, and RoomType {RoomType}",
            created.RoomId,
            created.RoomNumber,
            created.RoomType);

        return MapToResponse(created);
    }

    public async Task<RoomResponseDto?> UpdateAsync(int id, UpdateRoomDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
        {
            _logger.LogWarning(
                "Failed to update room. Room with RoomId {RoomId} was not found",
                id);
            return null;
        }

        var roomNumber = dto.RoomNumber.Trim();
        var status = NormalizeStatus(dto.Status) ?? "Available";

        var duplicate = await _repository.GetByRoomNumberAsync(roomNumber);

        if (duplicate is not null && duplicate.RoomId != id)
        {
            _logger.LogWarning(
                "Failed to update room {RoomId}. RoomNumber {RoomNumber} already belongs to another room",
                id,
                roomNumber);
            throw new ConflictException("Room number already exists.");
        }

        var room = new Room
        {
            RoomNumber = roomNumber,
            RoomType = dto.RoomType.Trim(),
            Capacity = dto.Capacity,
            Status = status
        };

        var updated = await _repository.UpdateAsync(id, room);

        if (updated is null)
        {
            _logger.LogWarning(
                "Failed to update room. Room with RoomId {RoomId} was not found during persist",
                id);
            return null;
        }

        _logger.LogInformation(
            "Room {RoomId} updated successfully with RoomNumber {RoomNumber} and RoomType {RoomType}",
            updated.RoomId,
            updated.RoomNumber,
            updated.RoomType);

        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning(
                "Failed to delete room. Room with RoomId {RoomId} was not found",
                id);
            return false;
        }

        _logger.LogInformation(
            "Room {RoomId} deleted successfully",
            id);

        return true;
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
