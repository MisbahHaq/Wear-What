using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Models;
using Shop.Data;
using System.Diagnostics;

namespace Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.CategoryId = new SelectList(_context.ShopCategories, "Id", "Name");
            ViewBag.Categories = await _context.ShopCategories.ToListAsync();
            ViewBag.Banners = await _context.Banners
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var bestSellers = await GetWeeklyBestSellersAsync();
            ViewBag.BestSellers = bestSellers;

            var officeFurnitureCat = await _context.ShopCategories.FirstOrDefaultAsync(c => c.Name == "Upgrade your office furniture");
            if (officeFurnitureCat != null)
            {
                ViewBag.OfficeFurnitureProducts = await _context.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == officeFurnitureCat.Id)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToListAsync();
                ViewBag.OfficeFurnitureCategoryId = officeFurnitureCat.Id;
            }

            var booksCat = await _context.ShopCategories.FirstOrDefaultAsync(c => c.Name == "Books you can't put down");
            if (booksCat != null)
            {
                ViewBag.BooksProducts = await _context.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == booksCat.Id)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToListAsync();
                ViewBag.BooksCategoryId = booksCat.Id;
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<List<Product>> GetWeeklyBestSellersAsync()
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            var topProductIds = await _context.OrderItems
                .Where(oi => oi.Order != null && oi.Order.CreatedAt >= oneWeekAgo)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(8)
                .Select(x => x.ProductId)
                .ToListAsync();

            var products = await _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .Where(p => topProductIds.Contains(p.Id))
                .ToListAsync();

            // Preserve the sold-rank ordering
            var ranked = topProductIds
                .Select(id => products.FirstOrDefault(p => p.Id == id))
                .OfType<Product>()
                .ToList();

            // Fallback to newest products if nothing sold this week
            if (!ranked.Any())
            {
                ranked = await _context.Products
                    .Include(p => p.Shop)
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToListAsync();
            }

            return ranked;
        }
    }
}
