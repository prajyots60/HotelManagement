using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.DTOs.Staff;

public class CreateStaffDto
{
    [Required, MaxLength(30)]
    public string StaffCode { get; set; } = string.Empty;

    [Required]
    public int DepartmentId { get; set; }

    [Required, MaxLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string NIC { get; set; } = string.Empty;

    [Range(0, 99999999.99)]
    public decimal Salary { get; set; }

    [Range(18, 100)]
    public int Age { get; set; }

    [Required, MaxLength(80)]
    public string Occupation { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;
}
