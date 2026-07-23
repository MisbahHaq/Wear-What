using MultiVendor.Domain.Entities;

namespace MultiVendor.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public Guid? ShopId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsActive { get; set; } = true;
    public int HelpfulCount { get; set; }

    public Product Product { get; set; } = null!;
    public ProductVariant? Variant { get; set; }
    public Shop? Shop { get; set; }
}
