using Microsoft.AspNetCore.Identity;

namespace Shop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}