using HotelManagementService.DTOs.RoomRates;
using HotelManagementService.Exceptions;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HotelManagementService.Services.Implementations;

public class RoomRateService : IRoomRateService
{
    private readonly IRoomRateRepository _repository;
    private readonly ILogger<RoomRateService> _logger;

    public RoomRateService(
        IRoomRateRepository repository,
        ILogger<RoomRateService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<RoomRateResponseDto>> GetAllAsync()
    {
        var rates = await _repository.GetAllAsync();
        return rates.Select(MapToResponse).ToList();
    }

    public async Task<RoomRateResponseDto?> GetByIdAsync(int id)
    {
        var rate = await _repository.GetByIdAsync(id);
        return rate is null ? null : MapToResponse(rate);
    }

    public async Task<RoomRateResponseDto> CreateAsync(CreateRoomRateDto dto)
    {
        if (!await _repository.RoomExistsAsync(dto.RoomId))
        {
            _logger.LogWarning(
                "Failed to create room rate. Room with RoomId {RoomId} does not exist",
                dto.RoomId);
            throw new NotFoundException("The specified room does not exist.");
        }

        var rate = new RoomRate
        {
            RoomId = dto.RoomId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Day = dto.Day.Trim(),
            GuestCount = dto.GuestCount,
            FirstNightPrice = dto.FirstNightPrice,
            ExtensionPrice = dto.ExtensionPrice
        };

        var created = await _repository.CreateAsync(rate);

        _logger.LogInformation(
            "Room rate created successfully with RateId {RateId} for RoomId {RoomId} (Day: {Day}, GuestCount: {GuestCount})",
            created.RateId,
            created.RoomId,
            created.Day,
            created.GuestCount);

        return MapToResponse(created);
    }

    public async Task<RoomRateResponseDto?> UpdateAsync(int id, UpdateRoomRateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
        {
            _logger.LogWarning(
                "Failed to update room rate. Room rate with RateId {RateId} was not found",
                id);
            return null;
        }

        if (!await _repository.RoomExistsAsync(dto.RoomId))
        {
            _logger.LogWarning(
                "Failed to update room rate {RateId}. Room with RoomId {RoomId} does not exist",
                id,
                dto.RoomId);
            throw new NotFoundException("The specified room does not exist.");
        }

        var rate = new RoomRate
        {
            RoomId = dto.RoomId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Day = dto.Day.Trim(),
            GuestCount = dto.GuestCount,
            FirstNightPrice = dto.FirstNightPrice,
            ExtensionPrice = dto.ExtensionPrice
        };

        var updated = await _repository.UpdateAsync(id, rate);

        if (updated is null)
        {
            _logger.LogWarning(
                "Failed to update room rate. Room rate with RateId {RateId} was not found during persist",
                id);
            return null;
        }

        _logger.LogInformation(
            "Room rate {RateId} updated successfully for RoomId {RoomId} (Day: {Day}, GuestCount: {GuestCount})",
            updated.RateId,
            updated.RoomId,
            updated.Day,
            updated.GuestCount);

        return MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _repository.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning(
                "Failed to delete room rate. Room rate with RateId {RateId} was not found",
                id);
            return false;
        }

        _logger.LogInformation(
            "Room rate {RateId} deleted successfully",
            id);

        return true;
    }

    private static RoomRateResponseDto MapToResponse(RoomRate rate)
    {
        return new RoomRateResponseDto
        {
            RateId = rate.RateId,
            RoomId = rate.RoomId,
            RoomNumber = rate.Room?.RoomNumber,
            StartDate = rate.StartDate,
            EndDate = rate.EndDate,
            Day = rate.Day,
            GuestCount = rate.GuestCount,
            FirstNightPrice = rate.FirstNightPrice,
            ExtensionPrice = rate.ExtensionPrice
        };
    }
}
