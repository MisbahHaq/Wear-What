using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;

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
    }

    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalShops { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
    }
}