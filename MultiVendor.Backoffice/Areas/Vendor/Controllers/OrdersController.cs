using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;
using MultiVendor.Core;

namespace MultiVendor.Backoffice.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(Roles = "Vendor")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private const int ReturnWindowDays = 14;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

        private async Task AddStatusHistoryAsync(int orderId, string status, string? note = null)
        {
            var userId = GetCurrentUserId();
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = orderId,
                Status = status,
                ChangedByUserId = userId,
                Timestamp = DateTime.UtcNow,
                Note = note
            });
            await _context.SaveChangesAsync();
        }

        private async Task SendNotificationAsync(string userId, string message, string? link = null)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUserId = GetCurrentUserId();
            var user = await _userManager.FindByIdAsync(currentUserId);
            var isVendor = user != null && await _userManager.IsInRoleAsync(user, "Vendor");
            ViewBag.IsVendor = isVendor;

            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            if (!shopIds.Any() && !isVendor)
            {
                return RedirectToAction("Index", "Home");
            }

            var orderItems = await _context.OrderItems
                .Include(i => i.Order)
                .ThenInclude(o => o!.Customer)
                .Include(i => i.Product)
                .ThenInclude(p => p!.Shop)
                .Where(i => shopIds.Contains(i.Product!.ShopId))
                .OrderByDescending(i => i.Order!.CreatedAt)
                .ToListAsync();

            var shops = await _context.Shops
                .Where(s => shopIds.Contains(s.Id))
                .ToListAsync();

            var lowStockProducts = await _context.Products
                .Where(p => shopIds.Contains(p.ShopId) && p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            var customerOrders = await _context.Orders
                .Where(o => o.CustomerId == currentUserId)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.Status,
                    o.TotalAmount,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            var viewModel = new OwnerDashboardViewModel
            {
                Shops = shops,
                OrderItems = orderItems,
                TotalSales = orderItems.Sum(i => i.UnitPrice * i.Quantity)
            };

            ViewBag.CustomerOrders = customerOrders;
            ViewBag.CustomerOrderCount = customerOrders.Count;
            ViewBag.LowStockProducts = lowStockProducts;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderItemId, string status)
        {
            var currentUserId = GetCurrentUserId();
            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            var orderItem = await _context.OrderItems
                .Include(i => i.Product)
                .Include(i => i.Order)
                .FirstOrDefaultAsync(i => i.Id == orderItemId);

            if (orderItem == null || orderItem.Product == null || !shopIds.Contains(orderItem.Product.ShopId))
                return Forbid();

            if (orderItem.Order != null)
            {
                orderItem.Order.Status = status;
                await _context.SaveChangesAsync();
                await AddStatusHistoryAsync(orderItem.Order.Id, status, "Status updated by seller");
                TempData["StatusMessage"] = "Order status updated.";

                if (orderItem.Order.CustomerId != null)
                {
                    await SendNotificationAsync(orderItem.Order.CustomerId, $"Order #{orderItem.Order.Id} status updated to {status}", $"/Orders/Tracking/{orderItem.Order.Id}");
                }
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Analytics()
        {
            var currentUserId = GetCurrentUserId();
            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

            if (!shopIds.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var now = DateTime.UtcNow;
            var thirtyDaysAgo = now.AddDays(-30);

            var orderItems = await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Product)
                .Where(i => shopIds.Contains(i.Product!.ShopId) && i.Order!.CreatedAt >= thirtyDaysAgo)
                .ToListAsync();

            var revenueByDay = orderItems
                .GroupBy(i => i.Order!.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.UnitPrice * x.Quantity) })
                .OrderBy(x => x.Date)
                .ToList();

            var topProducts = orderItems
                .GroupBy(i => i.Product!.Name)
                .Select(g => new { Product = g.Key, Quantity = g.Sum(x => x.Quantity), Revenue = g.Sum(x => x.UnitPrice * x.Quantity) })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            ViewBag.RevenueByDay = System.Text.Json.JsonSerializer.Serialize(revenueByDay);
            ViewBag.TopProducts = topProducts;
            ViewBag.TotalRevenue = revenueByDay.Sum(x => x.Revenue);
            ViewBag.TotalOrders = orderItems.Select(i => i.OrderId).Distinct().Count();

            return View();
        }
    }
}
