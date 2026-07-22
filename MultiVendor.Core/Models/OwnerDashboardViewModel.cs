namespace MultiVendor.Core.Models;

public class OwnerDashboardViewModel
{
    public List<ShopItem> Shops { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
    public decimal TotalSales { get; set; }
}
