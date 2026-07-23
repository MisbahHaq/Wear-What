using System;
using System.Collections.Generic;

namespace Nexora.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DeliveryMethod { get; set; }
        public string? PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
    }
}
