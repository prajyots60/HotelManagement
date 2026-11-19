namespace HotelManagementService.DTOs.Departments;

public class UpdateDepartmentDto
{
    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
