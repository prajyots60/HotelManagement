namespace HotelManagementService.DTOs.Inventory;

public class CreateInventoryDto
{
    public string ItemName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string Unit { get; set; } = string.Empty;

    public string? Description { get; set; }
}
