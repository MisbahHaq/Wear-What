using MultiVendor.DTOs;
using MultiVendor.Models;

namespace MultiVendor.Services.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutResultDto> ProcessCheckoutAsync(string customerId, CheckoutDto checkoutDto);
}

public class CheckoutResultDto
{
    public bool IsSuccess { get; set; }
    public string? OrderNumber { get; set; }
    public int OrderId { get; set; }
    public decimal GrandTotal { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<int, string> ShopOrderNumbers { get; set; } = new();
}
