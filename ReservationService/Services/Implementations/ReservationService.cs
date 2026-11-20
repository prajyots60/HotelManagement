using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReservationService.DTOs.Reservations;
using ReservationService.Exceptions;
using ReservationService.Models;
using ReservationService.Repositories.Interfaces;
using ReservationService.Services.Interfaces;

namespace ReservationService.Services.Implementations;

/// <summary>
/// Implements the business logic for hotel bookings and room availability.
/// Coordinates between local ReservationDB and remote HotelManagementService over HTTP.
/// </summary>
public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ReservationService> _logger;

    // HashSet provides O(1) constant-time lookup for valid lifecycle states
    private static readonly HashSet<string> ValidStatuses =
    [
        "Pending",
        "Confirmed",
        "Cancelled",
        "CheckedIn",
        "CheckedOut"
    ];

    // Set of statuses that occupy/block room availability
    private static readonly HashSet<string> BlockingStatuses =
    [
        "Pending",
        "Confirmed",
        "CheckedIn"
    ];

    public ReservationService(
        IReservationRepository reservationRepository,
        IGuestRepository guestRepository,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ReservationService> logger)
    {
        _reservationRepository = reservationRepository;
        _guestRepository = guestRepository;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all reservations with guest information eagerly loaded.
    /// </summary>
    public async Task<List<ReservationResponseDto>> GetAllAsync()
    {
        var reservations = await _reservationRepository.GetAllAsync();

        return reservations
            .Select(r => MapToResponse(r, "Reservations retrieved successfully."))
            .ToList();
    }

    /// <summary>
    /// Retrieves a single reservation by ID.
    /// </summary>
    public async Task<ReservationResponseDto?> GetByIdAsync(int id)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);

        return reservation is null
            ? null
            : MapToResponse(reservation, "Reservation retrieved successfully.");
    }

    /// <summary>
    /// Coordinates the full reservation creation workflow:
    /// 1. Validates stay dates (CheckOut > CheckIn)
    /// 2. Validates guest existence in ReservationDB
    /// 3. Validates room existence & capacity via HTTP call to HotelManagementService (OUTSIDE transaction)
    /// 4. Begins explicit database transaction
    /// 5. Acquires exclusive transaction-owned SQL application lock on dto.RoomId
    /// 6. Checks ReservationDB for schedule overlaps on that room
    /// 7. Computes nights, generates booking code, sets status = 'Confirmed'
    /// 8. Persists to ReservationDB and commits transaction (releasing lock)
    /// </summary>
    public async Task<ReservationResponseDto> CreateAsync(
        CreateReservationDto dto)
    {
        ValidateStayDates(dto.CheckIn, dto.CheckOut);

        await ValidateGuestAsync(dto.GuestId);

        // Downstream HTTP query strictly OUTSIDE database transaction
        var room = await GetRoomAsync(dto.RoomId);

        if (room is null)
        {
            _logger.LogWarning("Reservation creation rejected: RoomId {RoomId} not found.", dto.RoomId);
            throw new NotFoundException("Room not found.");
        }

        ValidateRoom(room, dto.Adults, dto.Children);

        await using var transaction = await _reservationRepository.BeginTransactionAsync();
        try
        {
            // Acquire exclusive, transaction-owned application lock for requested room
            await _reservationRepository.AcquireRoomLockAsync(dto.RoomId);

            var hasOverlap = await _reservationRepository
                .HasOverlappingReservationAsync(
                    dto.RoomId,
                    dto.CheckIn,
                    dto.CheckOut);

            if (hasOverlap)
            {
                _logger.LogWarning(
                    "Reservation rejected due to room unavailability. RoomId: {RoomId}",
                    dto.RoomId);
                throw new ConflictException(
                    "The selected room is not available for the requested dates.");
            }

            var reservation = new Reservation
            {
                ReservationCode = GenerateReservationCode(),
                GuestId = dto.GuestId,
                RoomId = dto.RoomId,
                Adults = dto.Adults,
                Children = dto.Children,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                Nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber,
                Status = "Confirmed"
            };

            var created = await _reservationRepository.CreateAsync(reservation);
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Reservation created successfully. ReservationId: {ReservationId}, RoomId: {RoomId}",
                created.ReservationId,
                created.RoomId);

            return MapToResponse(created, "Reservation created successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Updates an existing reservation with concurrency protection and date overlap check excluding itself.
    /// Locks rooms in ascending order when changing room allocation.
    /// </summary>
    public async Task<ReservationResponseDto?> UpdateAsync(
        int id,
        UpdateReservationDto dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);

        if (reservation is null)
        {
            _logger.LogWarning("Reservation update rejected: ReservationId {ReservationId} not found.", id);
            return null;
        }

        ValidateStayDates(dto.CheckIn, dto.CheckOut);

        await ValidateGuestAsync(dto.GuestId);

        // Downstream HTTP query strictly OUTSIDE database transaction
        var room = await GetRoomAsync(dto.RoomId);

        if (room is null)
        {
            _logger.LogWarning("Reservation update rejected: RoomId {RoomId} not found.", dto.RoomId);
            throw new NotFoundException("Room not found.");
        }

        ValidateRoom(room, dto.Adults, dto.Children);

        var oldRoomId = reservation.RoomId;
        var newRoomId = dto.RoomId;
        var roomsToLock = new[] { oldRoomId, newRoomId }.Distinct().OrderBy(r => r).ToList();

        await using var transaction = await _reservationRepository.BeginTransactionAsync();
        try
        {
            // Acquire locks in ascending order to prevent deadlocks
            foreach (var roomId in roomsToLock)
            {
                await _reservationRepository.AcquireRoomLockAsync(roomId);
            }

            var hasOverlap = await _reservationRepository
                .HasOverlappingReservationAsync(
                    dto.RoomId,
                    dto.CheckIn,
                    dto.CheckOut,
                    id);

            if (hasOverlap)
            {
                _logger.LogWarning(
                    "Reservation update rejected due to room unavailability. ReservationId: {ReservationId}, TargetRoomId: {RoomId}",
                    id,
                    dto.RoomId);
                throw new ConflictException(
                    "The selected room is not available for the requested dates.");
            }

            reservation.GuestId = dto.GuestId;
            reservation.RoomId = dto.RoomId;
            reservation.Adults = dto.Adults;
            reservation.Children = dto.Children;
            reservation.CheckIn = dto.CheckIn;
            reservation.CheckOut = dto.CheckOut;
            reservation.Nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

            await _reservationRepository.UpdateAsync(reservation);
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Reservation updated successfully. ReservationId: {ReservationId}, RoomId: {RoomId}",
                reservation.ReservationId,
                reservation.RoomId);

            return MapToResponse(reservation, "Reservation updated successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Updates the reservation lifecycle status.
    /// If transitioning from a non-blocking status (Cancelled/CheckedOut) to a blocking status,
    /// locks the room and re-verifies availability.
    /// </summary>
    public async Task<ReservationResponseDto?> UpdateStatusAsync(
        int id,
        UpdateReservationStatusDto dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);

        if (reservation is null)
        {
            _logger.LogWarning("Reservation status update rejected: ReservationId {ReservationId} not found.", id);
            return null;
        }

        if (!ValidStatuses.Contains(dto.Status))
        {
            _logger.LogWarning(
                "Reservation status update rejected due to invalid status value. ReservationId: {ReservationId}",
                id);
            throw new BadRequestException("Invalid reservation status.");
        }

        var wasBlocking = BlockingStatuses.Contains(reservation.Status);
        var willBeBlocking = BlockingStatuses.Contains(dto.Status);

        // If reactivating a non-blocking booking into a blocking status, acquire lock and verify availability
        if (!wasBlocking && willBeBlocking)
        {
            await using var transaction = await _reservationRepository.BeginTransactionAsync();
            try
            {
                await _reservationRepository.AcquireRoomLockAsync(reservation.RoomId);

                var hasOverlap = await _reservationRepository
                    .HasOverlappingReservationAsync(
                        reservation.RoomId,
                        reservation.CheckIn,
                        reservation.CheckOut,
                        id);

                if (hasOverlap)
                {
                    _logger.LogWarning(
                        "Reservation reactivation rejected due to room unavailability. ReservationId: {ReservationId}, RoomId: {RoomId}",
                        id,
                        reservation.RoomId);
                    throw new ConflictException(
                        "Cannot reactivate reservation: The room is no longer available for the scheduled dates.");
                }

                reservation.Status = dto.Status;
                await _reservationRepository.UpdateAsync(reservation);
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Reservation status changed successfully. ReservationId: {ReservationId}, NewStatus: {Status}",
                    reservation.ReservationId,
                    reservation.Status);

                return MapToResponse(
                    reservation,
                    "Reservation status updated successfully.");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Standard status update (e.g. Confirmed -> CheckedIn -> CheckedOut or Cancelled)
        reservation.Status = dto.Status;
        await _reservationRepository.UpdateAsync(reservation);

        _logger.LogInformation(
            "Reservation status changed successfully. ReservationId: {ReservationId}, NewStatus: {Status}",
            reservation.ReservationId,
            reservation.Status);

        return MapToResponse(
            reservation,
            "Reservation status updated successfully.");
    }

    /// <summary>
    /// Searches for available rooms matching date range and guest count:
    /// 1. Fetches all rooms from HotelManagementService over HTTP
    /// 2. Filters out rooms under Maintenance and rooms whose Capacity < guest count
    /// 3. Queries ReservationDB for any overlapping reservations for those candidate rooms
    /// 4. Returns only unbooked eligible rooms
    /// </summary>
    public async Task<List<RoomAvailabilityResponseDto>> GetAvailableRoomsAsync(
        DateOnly checkIn,
        DateOnly checkOut,
        int guests)
    {
        ValidateStayDates(checkIn, checkOut);

        if (guests <= 0)
            throw new BadRequestException("Guest count must be greater than zero.");

        var rooms = await GetAllRoomsAsync();

        // Maintenance rooms cannot be booked under any circumstances
        var eligibleRooms = rooms
            .Where(r =>
                r.Capacity >= guests &&
                !string.Equals(
                    r.Status,
                    "Maintenance",
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        var roomIds = eligibleRooms.Select(r => r.RoomId);

        // Single batch query in ReservationDB for all overlapping bookings across eligible rooms
        var overlappingReservations =
            await _reservationRepository.GetActiveReservationsForRoomsAsync(
                roomIds,
                checkIn,
                checkOut);

        var bookedRoomIds = overlappingReservations
            .Select(r => r.RoomId)
            .ToHashSet();

        // Filter out booked rooms
        return eligibleRooms
            .Where(r => !bookedRoomIds.Contains(r.RoomId))
            .Select(r => new RoomAvailabilityResponseDto
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                RoomType = r.RoomType,
                Capacity = r.Capacity,
                Status = r.Status
            })
            .ToList();
    }

    private async Task ValidateGuestAsync(int guestId)
    {
        var guest = await _guestRepository.GetByIdAsync(guestId);

        if (guest is null)
        {
            _logger.LogWarning("Reservation rejected: GuestId {GuestId} not found.", guestId);
            throw new NotFoundException("Guest not found.");
        }
    }

    private static void ValidateStayDates(
        DateOnly checkIn,
        DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new BadRequestException(
                "Check-out date must be later than check-in date.");
        }
    }

    private void ValidateRoom(
        HotelRoomResponseDto room,
        int adults,
        int children)
    {
        if (room.Capacity < adults + children)
        {
            _logger.LogWarning(
                "Reservation rejected due to room capacity. RoomId: {RoomId}, Capacity: {Capacity}, RequestedGuests: {RequestedGuests}",
                room.RoomId,
                room.Capacity,
                adults + children);
            throw new ConflictException(
                "The number of guests exceeds the room capacity.");
        }

        if (string.Equals(
            room.Status,
            "Maintenance",
            StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Reservation rejected due to room under maintenance. RoomId: {RoomId}",
                room.RoomId);
            throw new ConflictException(
                "The selected room is under maintenance.");
        }
    }

    /// <summary>
    /// Issues HTTP GET http://localhost:7002/api/rooms/{roomId} to HotelManagementService
    /// </summary>
    private async Task<HotelRoomResponseDto?> GetRoomAsync(int roomId)
    {
        try
        {
            using var request = CreateHotelRequest($"api/rooms/{roomId}");
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new DownstreamServiceException(
                    "HotelManagementService rejected the authentication token.",
                    response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new DownstreamServiceException(
                    $"HotelManagementService returned error status code: {(int)response.StatusCode}",
                    response.StatusCode);
            }

            return await response.Content.ReadFromJsonAsync<HotelRoomResponseDto>();
        }
        catch (DownstreamServiceException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new DownstreamServiceException(
                "Unable to connect to HotelManagementService.",
                ex,
                ex.StatusCode);
        }
    }

    /// <summary>
    /// Issues HTTP GET http://localhost:7002/api/rooms to HotelManagementService
    /// </summary>
    private async Task<List<HotelRoomResponseDto>> GetAllRoomsAsync()
    {
        try
        {
            using var request = CreateHotelRequest("api/rooms");
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new DownstreamServiceException(
                    "HotelManagementService rejected the authentication token.",
                    response.StatusCode);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new DownstreamServiceException(
                    $"HotelManagementService returned error status code: {(int)response.StatusCode}",
                    response.StatusCode);
            }

            return await response.Content
                .ReadFromJsonAsync<List<HotelRoomResponseDto>>()
                ?? [];
        }
        catch (DownstreamServiceException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new DownstreamServiceException(
                "Unable to connect to HotelManagementService.",
                ex,
                ex.StatusCode);
        }
    }

    /// <summary>
    /// Constructs an HttpRequestMessage and propagates the caller's incoming 'Authorization: Bearer <token>' header.
    /// This ensures secure service-to-service communication without needing a separate machine account.
    /// </summary>
    private HttpRequestMessage CreateHotelRequest(string relativeUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);

        var authorizationHeader = _httpContextAccessor
            .HttpContext?
            .Request.Headers["Authorization"]
            .ToString();

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            request.Headers.TryAddWithoutValidation(
                "Authorization",
                authorizationHeader);
        }

        return request;
    }

    /// <summary>
    /// Generates human-readable, unique booking code: RES-YYYYMMDD-12HEXCHARS (e.g. RES-20261119-A3F18C9D2E0B)
    /// </summary>
    private static string GenerateReservationCode()
    {
        return $"RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }

    private static ReservationResponseDto MapToResponse(
        Reservation reservation,
        string message)
    {
        return new ReservationResponseDto
        {
            Message = message,
            ReservationId = reservation.ReservationId,
            ReservationCode = reservation.ReservationCode,
            GuestId = reservation.GuestId,
            RoomId = reservation.RoomId,
            Adults = reservation.Adults,
            Children = reservation.Children,
            CheckIn = reservation.CheckIn,
            CheckOut = reservation.CheckOut,
            Nights = reservation.Nights,
            Status = reservation.Status
        };
    }

    /// <summary>
    /// Anti-Corruption Layer (ACL): Local private representation of HotelManagementService's RoomResponseDto.
    /// Prevents ReservationService from taking a compile-time dependency on HotelManagementService's project or dll.
    /// </summary>
    private sealed class HotelRoomResponseDto
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
