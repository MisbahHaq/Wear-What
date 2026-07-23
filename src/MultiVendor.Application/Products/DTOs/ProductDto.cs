namespace MultiVendor.Application.Products.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string? MainImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}
