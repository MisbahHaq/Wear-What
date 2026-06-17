using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiVendor.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int ShopOrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [MaxLength(300)]
    public string ProductNameSnapshot { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProductImageSnapshot { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    public ShopOrder ShopOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
