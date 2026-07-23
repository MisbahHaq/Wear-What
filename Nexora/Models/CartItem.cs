namespace Nexora.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; } = string.Empty;
        public string? Color { get; set; } = string.Empty;
    }
}