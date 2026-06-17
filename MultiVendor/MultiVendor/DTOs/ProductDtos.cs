using System.ComponentModel.DataAnnotations;

namespace MultiVendor.DTOs;

public class CreateProductDto
{
    [Required]
    [MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    [MaxLength(200)]
    public string? Sku { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }
}

public class UpdateProductDto
{
    [MaxLength(300)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? ShortDescription { get; set; }

    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public int? Stock { get; set; }

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public bool? IsActive { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
