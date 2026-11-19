using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Models;
using ReservationService.Repositories.Interfaces;

namespace ReservationService.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IReservationRepository interacting with [ReservationDB].[dbo].[Reservations].
/// Handles interval overlap mathematics and batch availability lookups.
/// </summary>
public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _context;

    public ReservationRepository(ReservationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Fetches all reservations with AsNoTracking() for high read throughput.
    /// Uses .Include() to eagerly load the associated Guest in a single SQL JOIN.
    /// </summary>
    public async Task<List<Reservation>> GetAllAsync()
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(reservation => reservation.Guest)
            .OrderByDescending(reservation => reservation.CheckIn)
            .ToListAsync();
    }

    /// <summary>
    /// Fetches a single reservation by ID.
    /// Notice: This query DOES NOT use AsNoTracking().
    /// EF Core's ChangeTracker monitors this entity so any property updates in the service layer
    /// are automatically tracked and flushed on SaveChangesAsync().
    /// </summary>
    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .Include(reservation => reservation.Guest)
            .FirstOrDefaultAsync(reservation =>
                reservation.ReservationId == id);
    }

    /// <summary>
    /// Checks if a room has an active booking that overlaps the half-open interval [checkIn, checkOut).
    /// Mathematical rule for overlap: (CheckIn < requestedCheckOut) AND (requestedCheckIn < CheckOut).
    /// Cancelled and CheckedOut reservations are excluded as they do not occupy room inventory.
    /// </summary>
    public async Task<bool> HasOverlappingReservationAsync(
        int roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        int? excludeReservationId = null)
    {
        var query = _context.Reservations
            .Where(reservation =>
                reservation.RoomId == roomId &&
                reservation.CheckIn < checkOut &&
                checkIn < reservation.CheckOut &&
                reservation.Status != "Cancelled" &&
                reservation.Status != "CheckedOut");

        // When updating an existing reservation, exclude itself from the collision check
        if (excludeReservationId.HasValue)
        {
            query = query.Where(reservation =>
                reservation.ReservationId != excludeReservationId.Value);
        }

        // AnyAsync translates to SQL: IF EXISTS (SELECT 1 FROM ...)
        return await query.AnyAsync();
    }

    /// <summary>
    /// Batch retrieves all overlapping reservations across multiple candidate rooms in a single SQL query.
    /// Uses SQL 'IN' clause: WHERE [RoomId] IN (1, 2, 3...)
    /// </summary>
    public async Task<List<Reservation>> GetActiveReservationsForRoomsAsync(
        IEnumerable<int> roomIds,
        DateOnly checkIn,
        DateOnly checkOut)
    {
        var roomIdList = roomIds.Distinct().ToList();

        if (roomIdList.Count == 0)
        {
            return new List<Reservation>();
        }

        return await _context.Reservations
            .AsNoTracking()
            .Where(reservation =>
                roomIdList.Contains(reservation.RoomId) &&
                reservation.CheckIn < checkOut &&
                checkIn < reservation.CheckOut &&
                reservation.Status != "Cancelled" &&
                reservation.Status != "CheckedOut")
            .ToListAsync();
    }

    /// <summary>
    /// Inserts a new reservation record.
    /// SQL Server outputs the auto-generated identity key into reservation.ReservationId.
    /// </summary>
    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    /// <summary>
    /// Persists changes to an existing tracked entity.
    /// Because GetByIdAsync returned a tracked instance, calling SaveChangesAsync()
    /// triggers snapshot change detection, generating an UPDATE for only modified columns.
    /// </summary>
    public async Task UpdateAsync(Reservation reservation)
    {
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Marks the reservation entity state as Deleted and issues a SQL DELETE statement.
    /// </summary>
    public async Task DeleteAsync(Reservation reservation)
    {
        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
    }
}
