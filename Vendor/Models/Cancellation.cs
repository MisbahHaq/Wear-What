using System;
using System.ComponentModel.DataAnnotations;

namespace Vendor.Models;

public class Cancellation
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Requested";

    [MaxLength(500)]
    public string? Reason { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }
}
