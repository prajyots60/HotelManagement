using ReservationService.Models;

namespace ReservationService.Repositories.Interfaces;

/// <summary>
/// Defines the contract for persistence operations on the Guest entity.
/// In .NET clean architecture, interfaces decouple the business layer from the database technology.
/// </summary>
public interface IGuestRepository
{
    /// <summary>
    /// Retrieves all guests as an IEnumerable. Read-only operation.
    /// </summary>
    Task<IEnumerable<Guest>> GetAllAsync();

    /// <summary>
    /// Finds a guest by primary key. Returns null if not found.
    /// The '?' denotes a nullable reference type introduced in C# 8.
    /// </summary>
    Task<Guest?> GetByIdAsync(int id);

    /// <summary>
    /// Searches for a guest by MemberCode for unique validation.
    /// </summary>
    Task<Guest?> GetByMemberCodeAsync(string memberCode);

    /// <summary>
    /// Adds a new guest to the database and returns the persisted entity (with generated GuestId).
    /// </summary>
    Task<Guest> CreateAsync(Guest guest);

    /// <summary>
    /// Updates an existing guest entity in the database context.
    /// </summary>
    Task UpdateAsync(Guest guest);

    /// <summary>
    /// Deletes a guest entity from the database context.
    /// </summary>
    Task DeleteAsync(Guest guest);
}
