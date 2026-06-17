using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Services.Interfaces;
using MultiVendor.ViewModels;

namespace MultiVendor.Services.Implementations;

public sealed class SessionCartService : ICartService
{
    private const string SessionKey = "cart";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ECommerceDbContext _context;

    public SessionCartService(IHttpContextAccessor httpContextAccessor, ECommerceDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public async Task<CartViewModel> GetCartAsync()
    {
        var items = GetStoredItems();
        if (items.Count == 0)
        {
            return new CartViewModel();
        }

        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Shop)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var productById = products.ToDictionary(p => p.Id);
        var cartItems = new List<CartItemResponseDto>();

        foreach (var item in items)
        {
            if (!productById.TryGetValue(item.ProductId, out var product) || !product.IsActive)
            {
                continue;
            }

            var quantity = Math.Min(item.Quantity, product.Stock);
            if (quantity <= 0)
            {
                continue;
            }

            cartItems.Add(new CartItemResponseDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ShopId = product.ShopId,
                ShopName = product.Shop.Name,
                UnitPrice = product.Price,
                Quantity = quantity,
                LineTotal = product.Price * quantity,
                AvailableStock = product.Stock,
                CoverImageUrl = product.CoverImageUrl
            });
        }

        var subtotal = cartItems.Sum(i => i.LineTotal);
        var shopCount = cartItems.Select(i => i.ShopId).Distinct().Count();
        var shipping = subtotal == 0 ? 0m : shopCount > 1 ? 9.99m : 0m;

        return new CartViewModel
        {
            Items = cartItems,
            Subtotal = subtotal,
            Shipping = shipping
        };
    }

    public async Task AddItemAsync(int productId, int quantity)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && p.IsActive)
            .SingleOrDefaultAsync();

        if (product is null)
        {
            throw new InvalidOperationException("Product is not available.");
        }

        var items = GetStoredItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId);
        var requestedQuantity = Math.Max(1, quantity);

        if (existing is null)
        {
            items.Add(new CartItemDto
            {
                ProductId = productId,
                Quantity = Math.Min(requestedQuantity, product.Stock)
            });
        }
        else
        {
            existing.Quantity = Math.Min(existing.Quantity + requestedQuantity, product.Stock);
        }

        SaveStoredItems(items);
    }

    public Task UpdateQuantityAsync(int productId, int quantity)
    {
        var items = GetStoredItems();
        var item = items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            return Task.CompletedTask;
        }

        item.Quantity = Math.Clamp(quantity, 1, 99);
        SaveStoredItems(items);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(int productId)
    {
        var items = GetStoredItems()
            .Where(i => i.ProductId != productId)
            .ToList();

        SaveStoredItems(items);
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        return Task.CompletedTask;
    }

    private List<CartItemDto> GetStoredItems()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return new List<CartItemDto>();
        }

        var json = session.GetString(SessionKey);
        return string.IsNullOrWhiteSpace(json)
            ? new List<CartItemDto>()
            : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();
    }

    private void SaveStoredItems(List<CartItemDto> items)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session is null)
        {
            return;
        }

        var json = JsonSerializer.Serialize(items);
        session.SetString(SessionKey, json);
    }
}
