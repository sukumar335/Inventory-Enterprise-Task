
namespace InventoryEnterpriseProject.Core.Entities;

public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = "";
    public string SKU { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
