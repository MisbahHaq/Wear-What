using System;
using System.ComponentModel.DataAnnotations;

namespace Vendor.Models;

public class WishlistItem
{
    public int Id { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
