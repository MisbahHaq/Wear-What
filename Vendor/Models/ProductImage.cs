using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vendor.Models;

public class ProductImage
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public Product? Product { get; set; }
}
