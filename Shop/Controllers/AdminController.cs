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

        public async Task<IActionResult> Banners()
        {
            var banners = await _context.Banners
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
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
    }
}