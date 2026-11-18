using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.DTOs.Inventory;

public class UpdateInventoryDto
{
    [Required(ErrorMessage = "Item name is required.")]
    [MaxLength(100, ErrorMessage = "Item name cannot exceed 100 characters.")]
    public string ItemName { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Unit is required.")]
    [MaxLength(30, ErrorMessage = "Unit cannot exceed 30 characters.")]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
    public string? Description { get; set; }
}
