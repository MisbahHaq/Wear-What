namespace Shop.Models
{
    public class LoginViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<ShopItem> Shops { get; set; } = new();
        public List<WishlistItem> WishlistItems { get; set; } = new();
    }

    public class OwnerDashboardViewModel
    {
        public List<ShopItem> Shops { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal TotalSales { get; set; }
    }

    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}