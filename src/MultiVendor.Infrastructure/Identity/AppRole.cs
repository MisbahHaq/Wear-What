using Microsoft.AspNetCore.Identity;

namespace MultiVendor.Domain.Entities;

public class AppRole : IdentityRole
{
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
