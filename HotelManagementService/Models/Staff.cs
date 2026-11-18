using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.Models;

public class Staff
{
    public int StaffId { get; set; }

    [Required]
    [MaxLength(30)]
    public string StaffCode { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EmployeeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string NIC { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public int Age { get; set; }

    [Required]
    [MaxLength(80)]
    public string Occupation { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    public Department? Department { get; set; }
}
