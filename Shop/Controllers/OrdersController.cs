using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private string GetCurrentUserId() => _userManager.GetUserId(User) ?? string.Empty;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int quantity, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null) return NotFound();

            if (quantity <= 0)
            {
                ModelState.AddModelError(nameof(quantity), "Quantity must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            var order = new Order
            {
                CustomerId = user.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                ShippingAddress = user.Address
            };

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = quantity,
                UnitPrice = product.Price
            });

            order.TotalAmount = product.Price * quantity;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["OrderMessage"] = "Order placed successfully.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Details", "Products", new { id = productId });
        }

        public async Task<IActionResult> Dashboard()
        {
            var currentUserId = GetCurrentUserId();
            var shopIds = await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();

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

            var viewModel = new OwnerDashboardViewModel
            {
                Shops = shops,
                OrderItems = orderItems,
                TotalSales = orderItems.Sum(i => i.UnitPrice * i.Quantity)
            };

            return View(viewModel);
        }
    }
}
