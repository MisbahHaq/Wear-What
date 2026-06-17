using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiVendor.Models;

public class ShopOrder
{
    public int Id { get; set; }

    [Required]
    public int ShopId { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    [MaxLength(50)]
    public string VendorOrderNumber { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [EnumDataType(typeof(VendorOrderStatus))]
    public VendorOrderStatus Status { get; set; } = VendorOrderStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal VendorTotal { get; set; }

    [MaxLength(50)]
    public string? TrackingNumber { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public Shop Shop { get; set; } = null!;
    public Order Order { get; set; } = null!;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public enum VendorOrderStatus
{
    Pending = 0,
    Accepted = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
