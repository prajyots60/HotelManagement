using System.ComponentModel.DataAnnotations;

namespace HotelManagementService.Models;

public class InventoryItem
{
    [Key]
    public int InventoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [Required]
    [MaxLength(30)]
    public string Unit { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Description { get; set; }
}
