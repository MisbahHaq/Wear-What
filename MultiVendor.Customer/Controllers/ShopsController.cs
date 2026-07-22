using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core.Data;
using MultiVendor.Core.Models;

namespace MultiVendor.Customer.Controllers
{
    public class ShopsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var shops = await _context.Shops.Include(s => s.Products).ToListAsync();
            return View(shops);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var shop = await _context.Shops
                .Include(s => s.Products)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shop == null) return NotFound();

            return View(shop);
        }
    }
}
