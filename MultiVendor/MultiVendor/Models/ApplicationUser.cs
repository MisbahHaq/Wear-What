using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MultiVendor.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsVendor { get; set; }

    public ICollection<Shop> Shops { get; set; } = new List<Shop>();
    public ICollection<Follow> Follows { get; set; } = new List<Follow>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
