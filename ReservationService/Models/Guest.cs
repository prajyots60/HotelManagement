using System.ComponentModel.DataAnnotations;

namespace ReservationService.Models;

public class Guest
{
    public int GuestId { get; set; }

    [Required]
    [MaxLength(30)]
    public string MemberCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Company { get; set; }

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    public ICollection<Reservation> Reservations { get; set; }
        = new List<Reservation>();
}
