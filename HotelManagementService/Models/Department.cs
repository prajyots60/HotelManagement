using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.Models;

public class Department
{
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }

    public ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
}
