namespace InventoryEnterpriseProject.Core.Entities;

public class Payment : BaseEntity
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = "Pending"; // Pending, Succeeded, Failed
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }
}
