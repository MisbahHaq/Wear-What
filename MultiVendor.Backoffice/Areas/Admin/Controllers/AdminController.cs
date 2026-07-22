using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;

namespace MultiVendor.Backoffice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Vendor")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string> GetCurrentUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? string.Empty;
        }

        private async Task<bool> IsVendorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, "Vendor");
        }

        private async Task<List<int>> GetVendorShopIdsAsync()
        {
            var currentUserId = await GetCurrentUserIdAsync();
            return await _context.Shops
                .Where(s => s.OwnerId == currentUserId)
                .Select(s => s.Id)
                .ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            if (await IsVendorAsync())
            {
                var shopIds = await GetVendorShopIdsAsync();
                var viewModel = new AdminDashboardViewModel
                {
                    TotalProducts = await _context.Products.Where(p => shopIds.Contains(p.ShopId)).CountAsync(),
                    TotalShops = shopIds.Count,
                    TotalCategories = await _context.ShopCategories.CountAsync(),
                    TotalUsers = await _context.Users.CountAsync()
                };
                return View(viewModel);
            }

            var adminViewModel = new AdminDashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalShops = await _context.Shops.CountAsync(),
                TotalCategories = await _context.ShopCategories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync()
            };
            return View(adminViewModel);
        }

        public async Task<IActionResult> Products(string? status = null)
        {
            if (await IsVendorAsync())
            {
                var shopIds = await GetVendorShopIdsAsync();
                var query = _context.Products
                    .Include(p => p.Shop)
                    .Where(p => shopIds.Contains(p.ShopId))
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(p => p.ModerationStatus == status);

                var products = await query.OrderByDescending(p => p.Id).ToListAsync();
                ViewBag.Status = status;
                return View(products);
            }

            var adminQuery = _context.Products.Include(p => p.Shop).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                adminQuery = adminQuery.Where(p => p.ModerationStatus == status);

            var allProducts = await adminQuery.OrderByDescending(p => p.Id).ToListAsync();
            ViewBag.Status = status;
            return View(allProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                if (await IsVendorAsync())
                {
                    var shopIds = await GetVendorShopIdsAsync();
                    if (!shopIds.Contains(product.ShopId))
                        return Forbid();
                }

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
                if (await IsVendorAsync())
                {
                    var shopIds = await GetVendorShopIdsAsync();
                    if (!shopIds.Contains(product.ShopId))
                        return Forbid();
                }

                product.ModerationStatus = "Rejected";
                product.RejectionReason = reason ?? "Rejected by admin";
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Product rejected.";
            }
            return RedirectToAction(nameof(Products));
        }

        public async Task<IActionResult> Shops(string? status = null)
        {
            if (await IsVendorAsync())
            {
                var currentUserId = await GetCurrentUserIdAsync();
                var query = _context.Shops
                    .Where(s => s.OwnerId == currentUserId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status) && status == "Suspended")
                    query = query.Where(s => s.IsSuspended);

                var shops = await query.OrderByDescending(s => s.Id).ToListAsync();
                ViewBag.Status = status;
                return View(shops);
            }

            var adminQuery = _context.Shops.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && status == "Suspended")
                adminQuery = adminQuery.Where(s => s.IsSuspended);

            var allShops = await adminQuery.OrderByDescending(s => s.Id).ToListAsync();
            ViewBag.Status = status;
            return View(allShops);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendShop(int id)
        {
            if (await IsVendorAsync())
            {
                var shopIds = await GetVendorShopIdsAsync();
                if (!shopIds.Contains(id))
                    return Forbid();
            }

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
            if (await IsVendorAsync())
            {
                var shopIds = await GetVendorShopIdsAsync();
                if (!shopIds.Contains(id))
                    return Forbid();
            }

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

            return View();
        }

        public async Task<IActionResult> ReturnRequests(string? status = null)
        {
            var shopIds = await GetVendorShopIdsAsync();
            var query = _context.ReturnRequests
                .Include(r => r.OrderItem)
                .ThenInclude(i => i.Product)
                .Include(r => r.Buyer)
                .Where(r => shopIds.Contains(r.OrderItem.Product.ShopId))
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

            var shopIds = await GetVendorShopIdsAsync();
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
                .Where(o => o.OrderItems.Any(i => shopIds.Contains(i.Product.ShopId)))
                .ToListAsync();

            var gmv = orders.Sum(o => o.TotalAmount);
            var orderCount = orders.Count;
            var customerCount = orders.Select(o => o.CustomerId).Distinct().Count();

            var byCategory = orders
                .SelectMany(o => o.OrderItems)
                .Where(i => i.Product != null && i.Product.CategoryId.HasValue && shopIds.Contains(i.Product.ShopId))
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
                .Where(i => i.Product != null && shopIds.Contains(i.Product.ShopId))
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
