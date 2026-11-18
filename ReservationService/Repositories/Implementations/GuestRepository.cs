using Microsoft.EntityFrameworkCore;
using ReservationService.Data;
using ReservationService.Models;
using ReservationService.Repositories.Interfaces;

namespace ReservationService.Repositories.Implementations;

/// <summary>
/// EF Core implementation of IGuestRepository.
/// Interacts directly with the ReservationDbContext and SQL Server table [Guests].
/// </summary>
public class GuestRepository : IGuestRepository
{
    // The DbContext represents a session with the database.
    // 'readonly' ensures this reference cannot be reassigned after construction.
    private readonly ReservationDbContext _context;

    // Constructor Injection: ASP.NET Core DI provides the scoped ReservationDbContext instance.
    public GuestRepository(ReservationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Asynchronously fetches all guests ordered by GuestId.
    /// Uses .AsNoTracking() to bypass the EF Core change tracker (improves speed & memory).
    /// </summary>
    public async Task<IEnumerable<Guest>> GetAllAsync()
    {
        return await _context.Guests
            .AsNoTracking()
            .OrderBy(guest => guest.GuestId)
            .ToListAsync();
    }

    /// <summary>
    /// Asynchronously retrieves a single guest by ID.
    /// Returns the first matching entity or null if none matches.
    /// Note: This entity IS tracked, so callers can modify its properties and save.
    /// </summary>
    public async Task<Guest?> GetByIdAsync(int id)
    {
        return await _context.Guests
            .FirstOrDefaultAsync(guest => guest.GuestId == id);
    }

    /// <summary>
    /// Queries the unique index on MemberCode.
    /// Translates to: SELECT TOP(1) * FROM [Guests] WHERE [MemberCode] = @memberCode
    /// </summary>
    public async Task<Guest?> GetByMemberCodeAsync(string memberCode)
    {
        return await _context.Guests
            .FirstOrDefaultAsync(guest => guest.MemberCode == memberCode);
    }

    /// <summary>
    /// Stages a new Guest entity into the change tracker and executes an INSERT statement.
    /// SQL Server populates the IDENTITY column (GuestId) back into the C# object.
    /// </summary>
    public async Task<Guest> CreateAsync(Guest guest)
    {
        await _context.Guests.AddAsync(guest);
        await _context.SaveChangesAsync();

        return guest;
    }

    /// <summary>
    /// Marks the entity state as Modified and persists changes via an UPDATE statement.
    /// </summary>
    public async Task UpdateAsync(Guest guest)
    {
        _context.Guests.Update(guest);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Marks the entity state as Deleted and executes a DELETE statement.
    /// If there are foreign key constraints (e.g., active reservations), SQL Server enforces them.
    /// </summary>
    public async Task DeleteAsync(Guest guest)
    {
        _context.Guests.Remove(guest);
        await _context.SaveChangesAsync();
    }
}
