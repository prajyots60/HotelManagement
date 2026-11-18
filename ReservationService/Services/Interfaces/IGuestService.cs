using ReservationService.DTOs.Guests;

namespace ReservationService.Services.Interfaces;

/// <summary>
/// Defines the business logic contract for guest operations.
/// In .NET clean architecture, the service layer orchestrates validation,
/// business rules, and mapping between DTOs and database models.
/// </summary>
public interface IGuestService
{
    /// <summary>
    /// Retrieves all guests mapped to read-only response DTOs.
    /// </summary>
    Task<IEnumerable<GuestResponseDto>> GetAllAsync();

    /// <summary>
    /// Finds a single guest by ID and maps to a response DTO. Returns null if not found.
    /// </summary>
    Task<GuestResponseDto?> GetByIdAsync(int id);

    /// <summary>
    /// Validates MemberCode uniqueness, constructs a Guest entity, and persists it.
    /// Throws InvalidOperationException if the MemberCode already exists.
    /// </summary>
    Task<GuestResponseDto> CreateAsync(CreateGuestDto dto);

    /// <summary>
    /// Updates an existing guest with self-collision checking. Returns null if not found.
    /// Throws InvalidOperationException if the MemberCode is taken by another guest.
    /// </summary>
    Task<GuestResponseDto?> UpdateAsync(int id, UpdateGuestDto dto);

    /// <summary>
    /// Deletes a guest by ID. Returns false if the record does not exist.
    /// </summary>
    Task<bool> DeleteAsync(int id);
}
