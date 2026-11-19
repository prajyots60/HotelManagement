using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using ReservationService.DTOs.Reservations;
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

    // HashSet provides O(1) constant-time lookup for valid lifecycle states
    private static readonly HashSet<string> ValidStatuses =
    [
        "Pending",
        "Confirmed",
        "Cancelled",
        "CheckedIn",
        "CheckedOut"
    ];

    public ReservationService(
        IReservationRepository reservationRepository,
        IGuestRepository guestRepository,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _reservationRepository = reservationRepository;
        _guestRepository = guestRepository;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
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
    /// 3. Validates room existence & capacity via HTTP call to HotelManagementService
    /// 4. Checks ReservationDB for schedule overlaps on that room
    /// 5. Computes nights, generates booking code, sets status = 'Confirmed'
    /// 6. Persists to ReservationDB
    /// </summary>
    public async Task<ReservationResponseDto> CreateAsync(
        CreateReservationDto dto)
    {
        ValidateStayDates(dto.CheckIn, dto.CheckOut);

        await ValidateGuestAsync(dto.GuestId);

        var room = await GetRoomAsync(dto.RoomId);

        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        ValidateRoom(room, dto.Adults, dto.Children);

        var hasOverlap = await _reservationRepository
            .HasOverlappingReservationAsync(
                dto.RoomId,
                dto.CheckIn,
                dto.CheckOut);

        if (hasOverlap)
            throw new InvalidOperationException(
                "The selected room is not available for the requested dates.");

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

        return MapToResponse(created, "Reservation created successfully.");
    }

    /// <summary>
    /// Updates an existing reservation with date overlap check excluding itself.
    /// </summary>
    public async Task<ReservationResponseDto?> UpdateAsync(
        int id,
        UpdateReservationDto dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);

        if (reservation is null)
            return null;

        ValidateStayDates(dto.CheckIn, dto.CheckOut);

        await ValidateGuestAsync(dto.GuestId);

        var room = await GetRoomAsync(dto.RoomId);

        if (room is null)
            throw new KeyNotFoundException("Room not found.");

        ValidateRoom(room, dto.Adults, dto.Children);

        var hasOverlap = await _reservationRepository
            .HasOverlappingReservationAsync(
                dto.RoomId,
                dto.CheckIn,
                dto.CheckOut,
                id);

        if (hasOverlap)
            throw new InvalidOperationException(
                "The selected room is not available for the requested dates.");

        reservation.GuestId = dto.GuestId;
        reservation.RoomId = dto.RoomId;
        reservation.Adults = dto.Adults;
        reservation.Children = dto.Children;
        reservation.CheckIn = dto.CheckIn;
        reservation.CheckOut = dto.CheckOut;
        reservation.Nights = dto.CheckOut.DayNumber - dto.CheckIn.DayNumber;

        await _reservationRepository.UpdateAsync(reservation);

        return MapToResponse(reservation, "Reservation updated successfully.");
    }

    /// <summary>
    /// Updates only the status field (e.g. Confirmed -> CheckedIn -> CheckedOut or Cancelled).
    /// </summary>
    public async Task<ReservationResponseDto?> UpdateStatusAsync(
        int id,
        UpdateReservationStatusDto dto)
    {
        var reservation = await _reservationRepository.GetByIdAsync(id);

        if (reservation is null)
            return null;

        if (!ValidStatuses.Contains(dto.Status))
            throw new ArgumentException("Invalid reservation status.");

        reservation.Status = dto.Status;

        await _reservationRepository.UpdateAsync(reservation);

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
            throw new ArgumentException("Guest count must be greater than zero.");

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
            throw new KeyNotFoundException("Guest not found.");
    }

    private static void ValidateStayDates(
        DateOnly checkIn,
        DateOnly checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new ArgumentException(
                "Check-out date must be later than check-in date.");
        }
    }

    private static void ValidateRoom(
        HotelRoomResponseDto room,
        int adults,
        int children)
    {
        if (room.Capacity < adults + children)
        {
            throw new InvalidOperationException(
                "The number of guests exceeds the room capacity.");
        }

        if (string.Equals(
            room.Status,
            "Maintenance",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The selected room is under maintenance.");
        }
    }

    /// <summary>
    /// Issues HTTP GET http://localhost:7002/api/rooms/{roomId} to HotelManagementService
    /// </summary>
    private async Task<HotelRoomResponseDto?> GetRoomAsync(int roomId)
    {
        using var request = CreateHotelRequest($"api/rooms/{roomId}");

        using var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (response.StatusCode is HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "HotelManagementService rejected the authentication token.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<HotelRoomResponseDto>();
    }

    /// <summary>
    /// Issues HTTP GET http://localhost:7002/api/rooms to HotelManagementService
    /// </summary>
    private async Task<List<HotelRoomResponseDto>> GetAllRoomsAsync()
    {
        using var request = CreateHotelRequest("api/rooms");

        using var response = await _httpClient.SendAsync(request);

        if (response.StatusCode is HttpStatusCode.Unauthorized
            or HttpStatusCode.Forbidden)
        {
            throw new UnauthorizedAccessException(
                "HotelManagementService rejected the authentication token.");
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<HotelRoomResponseDto>>()
            ?? [];
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
