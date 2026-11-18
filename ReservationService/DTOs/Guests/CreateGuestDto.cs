using System.ComponentModel.DataAnnotations;

namespace ReservationService.DTOs.Guests;

public class CreateGuestDto
{
    [Required(ErrorMessage = "Member code is required.")]
    [MaxLength(30, ErrorMessage = "Member code cannot exceed 30 characters.")]
    public string MemberCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone is required.")]
    [MaxLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Company cannot exceed 100 characters.")]
    public string? Company { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gender is required.")]
    [MaxLength(20, ErrorMessage = "Gender cannot exceed 20 characters.")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
    public string Address { get; set; } = string.Empty;
}
