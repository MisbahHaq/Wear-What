using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalShops = await _context.Shops.CountAsync(),
                TotalCategories = await _context.ShopCategories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync()
            };
            return View(viewModel);
        }

        public async Task<IActionResult> Products(string? status = null)
        {
            var query = _context.Products.Include(p => p.Shop).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.ModerationStatus == status);

            var products = await query.OrderByDescending(p => p.Id).ToListAsync();
            ViewBag.Status = status;
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.ModerationStatus = "Approved";
                product.RejectionReason = null;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Product approved.";
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProduct(int id, string? reason)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.ModerationStatus = "Rejected";
                product.RejectionReason = reason ?? "Rejected by admin";
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Product rejected.";
            }
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Shops(string? status = null)
        {
            var query = _context.Shops.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && status == "Suspended")
                query = query.Where(s => s.IsSuspended);

            var shops = await query.OrderByDescending(s => s.Id).ToListAsync();
            ViewBag.Status = status;
            return View(shops);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                shop.IsSuspended = true;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Shop suspended.";
            }
            return RedirectToAction(nameof(Shops));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsuspendShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                shop.IsSuspended = false;
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Shop unsuspended.";
            }
            return RedirectToAction(nameof(Shops));
        }

        public async Task<IActionResult> Banners()
        {
            var banners = await _context.Banners
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            ViewBag.Categories = await _context.ShopCategories.ToListAsync();
            return View(banners);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBanner(Banner banner)
        {
            if (ModelState.IsValid)
            {
                banner.CreatedAt = DateTime.UtcNow;
                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Banners));
            }

            var banners = await _context.Banners
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(nameof(Banners), banners);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner != null)
            {
                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Banners));
        }

        public async Task<IActionResult> EditBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound();
            return View(banner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBanner(int id, Banner banner)
        {
            if (id != banner.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.Banners.FindAsync(id);
                if (existing == null) return NotFound();

                existing.ImageUrl = banner.ImageUrl;
                existing.Title = banner.Title;
                existing.LinkUrl = banner.LinkUrl;
                existing.IsActive = banner.IsActive;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Banners));
            }

            return View(banner);
        }

        public async Task<IActionResult> ReturnRequests(string? status = null)
        {
            var query = _context.ReturnRequests
                .Include(r => r.OrderItem)
                .ThenInclude(i => i.Product)
                .Include(r => r.Buyer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status || r.RefundStatus == status);

            var requests = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
            ViewBag.Status = status;
            return View(requests);
        }

        public async Task<IActionResult> Reports(DateTime? from = null, DateTime? to = null)
        {
            var start = from?.Date ?? DateTime.UtcNow.AddDays(-30);
            var end = to?.Date.AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1);

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .ToListAsync();

            var gmv = orders.Sum(o => o.TotalAmount);
            var orderCount = orders.Count;
            var customerCount = orders.Select(o => o.CustomerId).Distinct().Count();

            var byCategory = orders
                .SelectMany(o => o.OrderItems)
                .Where(i => i.Product != null && i.Product.CategoryId.HasValue)
                .GroupBy(i => i.Product!.CategoryId!.Value)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Revenue = g.Sum(i => i.UnitPrice * i.Quantity),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var categoryNames = await _context.ShopCategories
                .Where(c => byCategory.Select(x => x.CategoryId).Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            var bySeller = orders
                .SelectMany(o => o.OrderItems)
                .Where(i => i.Product != null)
                .GroupBy(i => i.Product!.ShopId)
                .Select(g => new
                {
                    ShopId = g.Key,
                    Revenue = g.Sum(i => i.UnitPrice * i.Quantity),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            var shopNames = await _context.Shops
                .Where(s => bySeller.Select(x => x.ShopId).Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name);

            ViewBag.GMV = gmv;
            ViewBag.OrderCount = orderCount;
            ViewBag.CustomerCount = customerCount;
            ViewBag.ByCategory = byCategory;
            ViewBag.CategoryNames = categoryNames;
            ViewBag.BySeller = bySeller;
            ViewBag.ShopNames = shopNames;
            ViewBag.From = start;
            ViewBag.To = end;

            return View();
        }
    }
}