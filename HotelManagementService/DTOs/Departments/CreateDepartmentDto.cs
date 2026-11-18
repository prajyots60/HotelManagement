using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.DTOs.Departments;

public class CreateDepartmentDto
{
    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }
}
