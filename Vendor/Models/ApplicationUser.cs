using Microsoft.AspNetCore.Identity;

namespace Vendor.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
