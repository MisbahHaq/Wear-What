using MultiVendor.Domain.Entities;

namespace MultiVendor.Domain.Entities;

public class Discount : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public Guid? ShopId { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxUsageCount { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public string CreatedByAdminId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Shop? Shop { get; set; }
    public Category? Category { get; set; }
}
