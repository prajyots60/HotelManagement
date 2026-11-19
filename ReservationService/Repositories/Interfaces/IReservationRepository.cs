using ReservationService.Models;

namespace ReservationService.Repositories.Interfaces;

public interface IReservationRepository
{
    /// <summary>
    /// Retrieves all reservations ordered chronologically by CheckIn date.
    /// Eager-loads the associated Guest entity for reporting/UI display.
    /// </summary>
    Task<List<Reservation>> GetAllAsync();

    /// <summary>
    /// Retrieves a single reservation by ID as a TRACKED entity for subsequent modification.
    /// </summary>
    Task<Reservation?> GetByIdAsync(int id);

    /// <summary>
    /// Evaluates if any active booking conflicts with the requested [checkIn, checkOut) window for a room.
    /// Excludes 'Cancelled' and 'CheckedOut' records.
    /// Optional excludeReservationId is used during updates to avoid self-collision.
    /// </summary>
    Task<bool> HasOverlappingReservationAsync(
        int roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        int? excludeReservationId = null);

    /// <summary>
    /// Batch query: Returns all active reservations overlapping [checkIn, checkOut) for a list of candidate rooms.
    /// Used by the room availability search algorithm.
    /// </summary>
    Task<List<Reservation>> GetActiveReservationsForRoomsAsync(
        IEnumerable<int> roomIds,
        DateOnly checkIn,
        DateOnly checkOut);

    /// <summary>
    /// Inserts a new reservation and generates the ReservationId primary key.
    /// </summary>
    Task<Reservation> CreateAsync(Reservation reservation);

    /// <summary>
    /// Persists changes to an existing tracked reservation entity.
    /// </summary>
    Task UpdateAsync(Reservation reservation);

    /// <summary>
    /// Deletes a reservation from the database.
    /// </summary>
    Task DeleteAsync(Reservation reservation);
}
