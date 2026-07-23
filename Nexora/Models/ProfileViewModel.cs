using System.Collections.Generic;
using Nexora.Models;

namespace Nexora.Models
{
    public class ProfileViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserAddress { get; set; } = string.Empty;
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<Product> RecentlyViewed { get; set; } = new List<Product>();
    }
}