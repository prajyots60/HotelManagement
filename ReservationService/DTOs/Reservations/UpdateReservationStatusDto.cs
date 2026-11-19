using System.ComponentModel.DataAnnotations;

namespace ReservationService.DTOs.Reservations;

/// <summary>
/// Dedicated DTO for updating a reservation's lifecycle status.
/// Using a specific endpoint and DTO prevents clients from accidentally altering
/// booking dates, guest IDs, or room assignments when simply performing a check-in or cancellation.
/// </summary>
public class UpdateReservationStatusDto
{
    // Regex validation guarantees that only the 5 valid lifecycle states documented in the LLD are accepted.
    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression(
        "^(Pending|Confirmed|Cancelled|CheckedIn|CheckedOut)$",
        ErrorMessage = "Invalid reservation status. Allowed values: Pending, Confirmed, Cancelled, CheckedIn, CheckedOut.")]
    public string Status { get; set; } = string.Empty;
}
