using MultiVendor.Domain.Entities;

namespace MultiVendor.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ShopId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string? ProductName { get; set; }
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountedUnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? ThumbnailUrl { get; set; }

    public Order Order { get; set; } = null!;
    public Shop Shop { get; set; } = null!;
    public Product? Product { get; set; }
    public ProductVariant? ProductVariant { get; set; }
}
