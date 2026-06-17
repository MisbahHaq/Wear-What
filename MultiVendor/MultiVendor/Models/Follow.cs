using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiVendor.Models;

public class Follow
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(ApplicationUser))]
    public string FollowerId { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(Shop))]
    public int ShopId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Follower { get; set; } = null!;
    public Shop Shop { get; set; } = null!;
}
