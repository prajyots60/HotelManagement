using ReservationService.DTOs.Guests;
using ReservationService.Exceptions;
using ReservationService.Models;
using ReservationService.Repositories.Interfaces;
using ReservationService.Services.Interfaces;

namespace ReservationService.Services.Implementations;

/// <summary>
/// Implements the business logic for managing guests.
/// Acts as the intermediary between the Controller (HTTP boundary) and Repository (Data boundary).
/// </summary>
public class GuestService : IGuestService
{
    // Injected repository interface: allows switching out DB implementations or mocking in unit tests.
    private readonly IGuestRepository _guestRepository;

    public GuestService(IGuestRepository guestRepository)
    {
        _guestRepository = guestRepository;
    }

    /// <summary>
    /// Retrieves all guests and maps them using LINQ projection (MapToResponseDto).
    /// </summary>
    public async Task<IEnumerable<GuestResponseDto>> GetAllAsync()
    {
        var guests = await _guestRepository.GetAllAsync();

        // LINQ Select projects each Guest domain entity into a GuestResponseDto.
        return guests.Select(MapToResponseDto);
    }

    /// <summary>
    /// Retrieves a single guest by primary key.
    /// Uses C# conditional (ternary) operator to handle the null case cleanly.
    /// </summary>
    public async Task<GuestResponseDto?> GetByIdAsync(int id)
    {
        var guest = await _guestRepository.GetByIdAsync(id);

        return guest is null ? null : MapToResponseDto(guest);
    }

    /// <summary>
    /// Enforces uniqueness of MemberCode before inserting into the database.
    /// </summary>
    public async Task<GuestResponseDto> CreateAsync(CreateGuestDto dto)
    {
        // 1. Business Check: MemberCode must be unique across all guests
        var existingGuest = await _guestRepository
            .GetByMemberCodeAsync(dto.MemberCode);

        if (existingGuest is not null)
        {
            throw new ConflictException(
                "A guest with the provided member code already exists.");
        }

        // 2. Mapping DTO -> Domain Entity
        var guest = new Guest
        {
            MemberCode = dto.MemberCode,
            Name = dto.Name,
            Phone = dto.Phone,
            Company = dto.Company,
            Email = dto.Email,
            Gender = dto.Gender,
            Address = dto.Address
        };

        // 3. Persistence via Repository
        var createdGuest = await _guestRepository.CreateAsync(guest);

        // 4. Mapping Domain Entity -> Response DTO
        return MapToResponseDto(createdGuest);
    }

    /// <summary>
    /// Updates a guest, ensuring MemberCode does not collide with another guest's code.
    /// </summary>
    public async Task<GuestResponseDto?> UpdateAsync(
        int id,
        UpdateGuestDto dto)
    {
        // 1. Verify target guest exists
        var guest = await _guestRepository.GetByIdAsync(id);

        if (guest is null)
        {
            return null; // Signals 404 to the controller
        }

        // 2. Uniqueness check with self-exclusion:
        // If an existing guest has this MemberCode, but it's a DIFFERENT guest (GuestId != id), reject!
        var existingGuest = await _guestRepository
            .GetByMemberCodeAsync(dto.MemberCode);

        if (existingGuest is not null && existingGuest.GuestId != id)
        {
            throw new ConflictException(
                "A guest with the provided member code already exists.");
        }

        // 3. Mutate the tracked entity's properties
        guest.MemberCode = dto.MemberCode;
        guest.Name = dto.Name;
        guest.Phone = dto.Phone;
        guest.Company = dto.Company;
        guest.Email = dto.Email;
        guest.Gender = dto.Gender;
        guest.Address = dto.Address;

        // 4. Save changes via repository
        await _guestRepository.UpdateAsync(guest);

        return MapToResponseDto(guest);
    }

    /// <summary>
    /// Deletes a guest by ID. Returns false if the entity is not found.
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        var guest = await _guestRepository.GetByIdAsync(id);

        if (guest is null)
        {
            return false; // Signals 404 to the controller
        }

        await _guestRepository.DeleteAsync(guest);

        return true;
    }

    /// <summary>
    /// Pure function for transforming a Guest domain model into a GuestResponseDto.
    /// 'static' guarantees this helper method does not modify or depend on instance state.
    /// </summary>
    private static GuestResponseDto MapToResponseDto(Guest guest)
    {
        return new GuestResponseDto
        {
            GuestId = guest.GuestId,
            MemberCode = guest.MemberCode,
            Name = guest.Name,
            Phone = guest.Phone,
            Company = guest.Company,
            Email = guest.Email,
            Gender = guest.Gender,
            Address = guest.Address
        };
    }
}
