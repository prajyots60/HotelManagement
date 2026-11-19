namespace HotelManagementService.DTOs.Staff;

public class CreateStaffDto
{
    public string StaffCode { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string NIC { get; set; } = string.Empty;

    public decimal Salary { get; set; }

    public int Age { get; set; }

    public string Occupation { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
