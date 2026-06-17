namespace Vendor.Models;

public class ProductFormViewModel
{
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string BroadCategory { get; set; } = "Unisex";
    public int CategoryId { get; set; }
    public string? Image1 { get; set; }
    public string? Image2 { get; set; }
    public string? Image3 { get; set; }
    public string? Image4 { get; set; }
    public string? Image5 { get; set; }
}
