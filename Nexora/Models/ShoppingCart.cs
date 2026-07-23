using System;

namespace Nexora.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}