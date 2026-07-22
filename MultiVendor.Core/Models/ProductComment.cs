using System.ComponentModel.DataAnnotations;

namespace MultiVendor.Core.Models
{
    public class ProductComment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
