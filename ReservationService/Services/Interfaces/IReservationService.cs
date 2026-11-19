using ReservationService.DTOs.Reservations;

namespace ReservationService.Services.Interfaces;

/// <summary>
/// Business service interface orchestrating reservation workflows and cross-service room availability queries.
/// </summary>
public interface IReservationService
{
    /// <summary>
    /// Retrieves all reservations mapped to response DTOs.
    /// </summary>
    Task<List<ReservationResponseDto>> GetAllAsync();

    /// <summary>
    /// Retrieves a single reservation by ID. Returns null if not found.
    /// </summary>
    Task<ReservationResponseDto?> GetByIdAsync(int id);

    /// <summary>
    /// Validates guest, fetches room from HotelManagementService via HTTP, checks capacity,
    /// verifies non-overlapping availability, calculates stay duration, and persists as 'Confirmed'.
    /// </summary>
    Task<ReservationResponseDto> CreateAsync(CreateReservationDto dto);

    /// <summary>
    /// Updates stay dates, occupants, or room assignment with collision checking.
    /// </summary>
    Task<ReservationResponseDto?> UpdateAsync(
        int id,
        UpdateReservationDto dto);

    /// <summary>
    /// Updates the reservation lifecycle status (Pending, Confirmed, Cancelled, CheckedIn, CheckedOut).
    /// </summary>
    Task<ReservationResponseDto?> UpdateStatusAsync(
        int id,
        UpdateReservationStatusDto dto);

    /// <summary>
    /// Inter-service query: Fetches eligible rooms from HotelManagementService,
    /// checks ReservationDB for schedule overlaps, and returns available rooms.
    /// </summary>
    Task<List<RoomAvailabilityResponseDto>> GetAvailableRoomsAsync(
        DateOnly checkIn,
        DateOnly checkOut,
        int guests);
}
