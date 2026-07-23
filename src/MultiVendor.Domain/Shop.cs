using MultiVendor.Domain.Entities;

namespace MultiVendor.Domain.Entities;

public class Shop : BaseEntity
{
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ShopApprovalStatus ApprovalStatus { get; set; } = ShopApprovalStatus.Pending;
    public bool IsActive { get; set; } = true;
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByAdminId { get; set; }
    public string? RejectionReason { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Discount> Discounts { get; set; } = new List<Discount>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
