namespace Shop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }
        public ShopCategory? Category { get; set; }
        public string Niche { get; set; } = string.Empty;
        public string ImageUrl1 { get; set; } = string.Empty;
        public string ImageUrl2 { get; set; } = string.Empty;
        public string ImageUrl3 { get; set; } = string.Empty;
        public string ImageUrl4 { get; set; } = string.Empty;
        public string ImageUrl5 { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public ShopItem? Shop { get; set; }
        public List<ProductComment> Comments { get; set; } = new();
        public List<ProductSpecification> Specifications { get; set; } = new();
        public List<ProductColor> Colors { get; set; } = new();
    }
}