using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ReservationService.Data;
using ReservationService.Exceptions;
using ReservationService.Models;
using ReservationService.Repositories.Interfaces;

namespace ReservationService.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IReservationRepository interacting with [ReservationDB].[dbo].[Reservations].
/// Handles interval overlap mathematics, batch availability lookups, and transaction-owned application locking.
/// </summary>
public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _context;

    public ReservationRepository(ReservationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Begins an explicit EF Core database transaction.
    /// </summary>
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <summary>
    /// Acquires an exclusive, transaction-owned SQL Server application lock for the given room ID.
    /// Executes sys.sp_getapplock on the active transaction using DbCommand.
    /// Return codes:
    ///   0: The lock was granted synchronously.
    ///   1: The lock was granted after waiting for other locks to be released.
    ///  -1: The lock request timed out.
    ///  -2: The lock request was canceled.
    ///  -3: The lock request was chosen as a deadlock victim.
    /// -999: Indicates a parameter validation or other call error.
    /// </summary>
    public async Task AcquireRoomLockAsync(int roomId, CancellationToken cancellationToken = default)
    {
        var currentTransaction = _context.Database.CurrentTransaction;
        if (currentTransaction is null)
        {
            throw new InvalidOperationException("AcquireRoomLockAsync must be called within an active database transaction.");
        }

        var connection = _context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        using var command = connection.CreateCommand();
        command.Transaction = currentTransaction.GetDbTransaction();
        command.CommandText = "sys.sp_getapplock";
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = 10; // seconds

        var resourceParam = command.CreateParameter();
        resourceParam.ParameterName = "@Resource";
        resourceParam.Value = $"Reservation_Room_{roomId}";
        resourceParam.DbType = DbType.String;
        resourceParam.Size = 255;
        command.Parameters.Add(resourceParam);

        var lockModeParam = command.CreateParameter();
        lockModeParam.ParameterName = "@LockMode";
        lockModeParam.Value = "Exclusive";
        lockModeParam.DbType = DbType.String;
        lockModeParam.Size = 32;
        command.Parameters.Add(lockModeParam);

        var lockOwnerParam = command.CreateParameter();
        lockOwnerParam.ParameterName = "@LockOwner";
        lockOwnerParam.Value = "Transaction";
        lockOwnerParam.DbType = DbType.String;
        lockOwnerParam.Size = 32;
        command.Parameters.Add(lockOwnerParam);

        var lockTimeoutParam = command.CreateParameter();
        lockTimeoutParam.ParameterName = "@LockTimeout";
        lockTimeoutParam.Value = 5000; // 5000 ms timeout
        lockTimeoutParam.DbType = DbType.Int32;
        command.Parameters.Add(lockTimeoutParam);

        var returnParam = command.CreateParameter();
        returnParam.ParameterName = "@Result";
        returnParam.Direction = ParameterDirection.ReturnValue;
        returnParam.DbType = DbType.Int32;
        command.Parameters.Add(returnParam);

        await command.ExecuteNonQueryAsync(cancellationToken);

        var result = returnParam.Value is int intVal ? intVal : Convert.ToInt32(returnParam.Value);

        if (result == 0 || result == 1)
        {
            // Lock successfully acquired (immediately or after wait)
            return;
        }

        if (result == -1) // Timeout
        {
            throw new ConflictException("The selected room is currently being booked by another request. Please retry.");
        }

        if (result == -3) // Deadlock victim
        {
            throw new ConflictException("A concurrency conflict occurred while reserving the room. Please retry.");
        }

        throw new InvalidOperationException($"Failed to acquire application lock for room {roomId}. sp_getapplock returned status code {result}.");
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
