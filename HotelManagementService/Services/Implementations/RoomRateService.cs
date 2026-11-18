using HotelManagementService.DTOs.RoomRates;
using HotelManagementService.Models;
using HotelManagementService.Repositories.Interfaces;
using HotelManagementService.Services.Interfaces;

namespace HotelManagementService.Services.Implementations;

public class RoomRateService : IRoomRateService
{
    private readonly IRoomRateRepository _repository;

    public RoomRateService(IRoomRateRepository repository)
    {
        _repository = repository;
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

    public async Task<(RoomRateResponseDto? Data, string? Error)> CreateAsync(
        CreateRoomRateDto dto)
    {
        var error = await ValidateAsync(
            dto.RoomId, dto.StartDate, dto.EndDate,
            dto.GuestCount, dto.FirstNightPrice, dto.ExtensionPrice);

        if (error is not null)
            return (null, error);

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
        return (MapToResponse(created), null);
    }

    public async Task<(RoomRateResponseDto? Data, string? Error)> UpdateAsync(
        int id, UpdateRoomRateDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);

        if (existing is null)
            return (null, null);

        var error = await ValidateAsync(
            dto.RoomId, dto.StartDate, dto.EndDate,
            dto.GuestCount, dto.FirstNightPrice, dto.ExtensionPrice);

        if (error is not null)
            return (null, error);

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

        return (updated is null ? null : MapToResponse(updated), null);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _repository.DeleteAsync(id);
    }

    private async Task<string?> ValidateAsync(
        int roomId,
        DateOnly startDate,
        DateOnly endDate,
        int guestCount,
        decimal firstNightPrice,
        decimal extensionPrice)
    {
        if (!await _repository.RoomExistsAsync(roomId))
            return "The specified room does not exist.";

        if (startDate > endDate)
            return "Start date cannot be after end date.";

        if (guestCount <= 0)
            return "Guest count must be greater than zero.";

        if (firstNightPrice <= 0 || extensionPrice <= 0)
            return "Prices must be greater than zero.";

        return null;
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
