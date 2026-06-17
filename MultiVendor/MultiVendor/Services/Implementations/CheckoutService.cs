using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;
using MultiVendor.Services.Interfaces;

namespace MultiVendor.Services.Implementations;

public class CheckoutService : ICheckoutService
{
    private readonly ECommerceDbContext _context;

    public CheckoutService(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutResultDto> ProcessCheckoutAsync(string customerId, CheckoutDto checkoutDto)
    {
        var result = new CheckoutResultDto();

        if (checkoutDto.Items == null || !checkoutDto.Items.Any())
        {
            result.Errors.Add("Cart is empty.");
            return result;
        }

        var productIds = checkoutDto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Include(p => p.Shop)
            .ToListAsync();

        var validationErrors = ValidateCart(checkoutDto.Items, products);
        if (validationErrors.Any())
        {
            result.Errors.AddRange(validationErrors);
            return result;
        }

        var enrichedItems = EnrichCartItems(checkoutDto.Items, products);

        var groupedByShop = enrichedItems
            .GroupBy(i => i.ShopId)
            .OrderBy(g => g.Key)
            .ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var orderTotal = enrichedItems.Sum(i => i.LineTotal);
            var shippingFee = groupedByShop.Count == 1 ? 0m : 9.99m;
            var grandTotal = orderTotal + shippingFee;

            var order = new Order
            {
                CustomerId = customerId,
                OrderNumber = GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = checkoutDto.ShippingAddress,
                SubTotal = orderTotal,
                TaxAmount = 0,
                ShippingAmount = shippingFee,
                DiscountAmount = 0,
                GrandTotal = grandTotal
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var shopGroup in groupedByShop)
            {
                var shopOrderSubTotal = shopGroup.Sum(i => i.LineTotal);

                var shopOrder = new ShopOrder
                {
                    ShopId = shopGroup.Key,
                    OrderId = order.Id,
                    VendorOrderNumber = GenerateVendorOrderNumber(),
                    CreatedAt = DateTime.UtcNow,
                    Status = VendorOrderStatus.Pending,
                    SubTotal = shopOrderSubTotal,
                    VendorTotal = shopOrderSubTotal
                };

                _context.ShopOrders.Add(shopOrder);
                await _context.SaveChangesAsync();

                foreach (var item in shopGroup)
                {
                    var orderItem = new OrderItem
                    {
                        ShopOrderId = shopOrder.Id,
                        ProductId = item.ProductId,
                        ProductNameSnapshot = item.ProductName,
                        ProductImageSnapshot = item.CoverImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TaxRate = 0,
                        LineTotal = item.LineTotal
                    };

                    _context.OrderItems.Add(orderItem);
                }
            }

            foreach (var item in enrichedItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.Stock -= item.Quantity;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.IsSuccess = true;
            result.OrderId = order.Id;
            result.OrderNumber = order.OrderNumber;
            result.GrandTotal = order.GrandTotal;

            foreach (var shopGroup in groupedByShop)
            {
                var shopOrder = order.ShopOrders.First(so => so.ShopId == shopGroup.Key);
                result.ShopOrderNumbers[shopGroup.Key] = shopOrder.VendorOrderNumber;
            }

            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Errors.Add($"Checkout failed: {ex.Message}");
            return result;
        }
    }

    private List<string> ValidateCart(List<CartItemDto> cartItems, List<Product> products)
    {
        var errors = new List<string>();
        var productDict = products.ToDictionary(p => p.Id);

        foreach (var item in cartItems)
        {
            if (!productDict.TryGetValue(item.ProductId, out var product))
            {
                errors.Add($"Product ID {item.ProductId} not found.");
                continue;
            }

            if (!product.IsActive)
                errors.Add($"Product '{product.Name}' is no longer available.");

            if (product.Stock < item.Quantity)
                errors.Add($"Insufficient stock for '{product.Name}'. Available: {product.Stock}, Requested: {item.Quantity}.");
        }

        return errors;
    }

    private List<CartEnrichedItem> EnrichCartItems(List<CartItemDto> cartItems, List<Product> products)
    {
        var productDict = products.ToDictionary(p => p.Id);

        return cartItems.Select(item =>
        {
            var product = productDict[item.ProductId];
            return new CartEnrichedItem
            {
                ProductId = item.ProductId,
                ShopId = product.ShopId,
                ShopName = product.Shop.Name,
                ProductName = product.Name,
                CoverImageUrl = product.CoverImageUrl,
                UnitPrice = product.Price,
                Quantity = item.Quantity,
                LineTotal = product.Price * item.Quantity
            };
        }).ToList();
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20).ToUpperInvariant();
    }

    private string GenerateVendorOrderNumber()
    {
        return $"VON-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".Substring(0, 20).ToUpperInvariant();
    }

    private class CartEnrichedItem
    {
        public int ProductId { get; set; }
        public int ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string? CoverImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}
