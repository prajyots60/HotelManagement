namespace ReservationService.DTOs.Guests;

public class UpdateGuestDto
{
    public string MemberCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Company { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;
}
