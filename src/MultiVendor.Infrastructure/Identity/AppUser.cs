using Microsoft.AspNetCore.Identity;

namespace MultiVendor.Domain.Entities;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Shop? Shop { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<OrderStatusHistory> StatusChanges { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Payment> PaymentsProcessed { get; set; } = new List<Payment>();
    public ICollection<Discount> DiscountsCreated { get; set; } = new List<Discount>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
