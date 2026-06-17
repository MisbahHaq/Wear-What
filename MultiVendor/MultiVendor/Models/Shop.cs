using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiVendor.Models;

public class Shop
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(ApplicationUser))]
    public string OwnerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    public string? BannerUrl { get; set; }

    [MaxLength(200)]
    public string? Slug { get; set; }

    [MaxLength(100)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser Owner { get; set; } = null!;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<ShopOrder> ShopOrders { get; set; } = new List<ShopOrder>();
}
