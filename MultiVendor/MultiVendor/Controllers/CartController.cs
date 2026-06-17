using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Services.Interfaces;
using MultiVendor.ViewModels;
using System.Security.Claims;

namespace MultiVendor.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly ICheckoutService _checkoutService;
    private readonly ECommerceDbContext _context;

    public CartController(
        ICartService cartService,
        ICheckoutService checkoutService,
        ECommerceDbContext context)
    {
        _cartService = cartService;
        _checkoutService = checkoutService;
        _context = context;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartAsync();
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        try
        {
            await _cartService.AddItemAsync(productId, quantity);
            TempData["Success"] = "Product added to cart.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Product", new { id = productId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CartUpdateDto model, CancellationToken ct = default)
    {
        await _cartService.UpdateQuantityAsync(model.ProductId, model.Quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId, CancellationToken ct = default)
    {
        await _cartService.RemoveItemAsync(productId);
        TempData["Success"] = "Product removed from cart.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Checkout(CancellationToken ct = default)
    {
        var cart = await _cartService.GetCartAsync();
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutDto model, CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Unauthorized();
        }

        var cart = await _cartService.GetCartAsync();
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        model.Items = cart.Items
            .Select(i => new CartItemDto { ProductId = i.ProductId, Quantity = i.Quantity })
            .ToList();

        var result = await _checkoutService.ProcessCheckoutAsync(userId, model);
        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(cart);
        }

        await _cartService.ClearAsync();
        TempData["Success"] = $"Order {result.OrderNumber} placed successfully.";
        return RedirectToAction("Details", "Order", new { id = result.OrderId });
    }

    public async Task<IActionResult> MyOrders(CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null)
        {
            return Unauthorized();
        }

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == userId)
            .Include(o => o.ShopOrders)
            .ThenInclude(so => so.Shop)
            .Include(o => o.ShopOrders)
            .ThenInclude(so => so.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderDate,
                o.Status,
                o.GrandTotal,
                o.ShippingAddress,
                o.TrackingNumber,
                o.ShippedAt,
                o.DeliveredAt,
                ShopCount = o.ShopOrders.Count,
                ItemCount = o.ShopOrders.Sum(so => so.OrderItems.Sum(i => i.Quantity)),
                ShopOrders = o.ShopOrders.Select(so => new
                {
                    so.Id,
                    so.VendorOrderNumber,
                    so.Status,
                    so.SubTotal,
                    so.VendorTotal,
                    so.TrackingNumber,
                    ShopName = so.Shop.Name,
                    ItemCount = so.OrderItems.Sum(i => i.Quantity)
                }).ToList()
            })
            .ToListAsync(ct);

        return View(orders);
    }
}
