using Microsoft.AspNetCore.Identity;

namespace MultiVendor.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? CNIC { get; set; }
        public string? ContactNumber { get; set; }
        public string? ShopName { get; set; }
    }
}
