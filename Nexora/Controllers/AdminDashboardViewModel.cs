using Nexora.Models;

namespace Nexora.Controllers
{
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int PendingOrders { get; set; }
        public int OrderReceivedOrders { get; set; }
        public int OutForDeliveryOrders { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> RecentProducts { get; set; } = new();
        public Product? BestSellingProduct { get; set; }
        public int BestSellingProductQuantity { get; set; }
        public Product? MostBookmarkedProduct { get; set; }
        public int MostBookmarkedProductCount { get; set; }
        public int PendingCancellationRequests { get; set; }
        public int NewChatRequests { get; set; }
        public int UnreadSupportRequests { get; set; }
        public List<SupportRequest> RecentSupportRequests { get; set; } = new();
    }
}