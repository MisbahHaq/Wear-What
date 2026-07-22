using System.ComponentModel.DataAnnotations;

namespace Shop.Models
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required]
        public string UserRole { get; set; } = "Customer";

        public string CNIC { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            Console.WriteLine($"[DEBUG] Validate called: UserRole='{UserRole}', CNIC='{CNIC}', ContactNumber='{ContactNumber}', ShopName='{ShopName}'");
            if (UserRole == "Vendor")
            {
                if (string.IsNullOrWhiteSpace(CNIC))
                    yield return new ValidationResult("CNIC is required for vendors.", new[] { nameof(CNIC) });

                if (string.IsNullOrWhiteSpace(ContactNumber))
                    yield return new ValidationResult("Contact number is required for vendors.", new[] { nameof(ContactNumber) });

                if (string.IsNullOrWhiteSpace(ShopName))
                    yield return new ValidationResult("Shop name is required for vendors.", new[] { nameof(ShopName) });
            }
        }
    }

    public class ProfileViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public List<ShopItem> Shops { get; set; } = new();
        public List<WishlistItem> WishlistItems { get; set; } = new();
        public List<UserOrderSummary> Orders { get; set; } = new();
    }

    public class UserOrderSummary
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
    }

    public class OwnerDashboardViewModel
    {
        public List<ShopItem> Shops { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
        public decimal TotalSales { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class OrderTrackingViewModel
    {
        public Order Order { get; set; } = new Order();
        public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    }
}