using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Vendor.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    public List<Address> Addresses { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<WishlistItem> WishlistItems { get; set; } = new();
}
