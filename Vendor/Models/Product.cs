using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vendor.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    [MaxLength(20)]
    public string BroadCategory { get; set; } = "Unisex";

    public int CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    public List<ProductImage> Images { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ShopId { get; set; }
    public Shop? Shop { get; set; }
}
