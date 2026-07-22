namespace MultiVendor.Core.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        public bool IsAvailable => StockQuantity > 0;
        public int? CategoryId { get; set; }
        public ShopCategory? Category { get; set; }
        public string? Niche { get; set; }
        public string ModerationStatus { get; set; } = "Approved";
        public string? RejectionReason { get; set; }
        public string? ImageUrl1 { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }
        public string? ImageUrl4 { get; set; }
        public string? ImageUrl5 { get; set; }
        public int ShopId { get; set; }
        public ShopItem? Shop { get; set; }
        public List<ProductComment> Comments { get; set; } = new();
        public List<ProductSpecification> Specifications { get; set; } = new();
        public List<ProductColor> Colors { get; set; } = new();
    }
}
