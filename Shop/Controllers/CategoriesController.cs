using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? shopId)
        {
            var query = _context.ShopCategories.Include(c => c.Shop).AsQueryable();
            if (shopId.HasValue)
                query = query.Where(c => c.ShopId == shopId.Value);
            var categories = await query.ToListAsync();
            ViewBag.ShopId = new SelectList(_context.Shops, "Id", "Name", shopId);
            return View(categories);
        }

        public async Task<IActionResult> Create(int? shopId)
        {
            ViewBag.ShopId = new SelectList(_context.Shops, "Id", "Name", shopId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { shopId = category.ShopId });
            }
            ViewBag.ShopId = new SelectList(_context.Shops, "Id", "Name", category.ShopId);
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.ShopCategories.FindAsync(id);
            if (category == null) return NotFound();

            ViewBag.ShopId = new SelectList(_context.Shops, "Id", "Name", category.ShopId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShopCategory category)
        {
            if (id != category.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.ShopCategories.FindAsync(id);
                    if (existingCategory == null) return NotFound();

                    existingCategory.Name = category.Name;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { shopId = category.ShopId });
            }
            ViewBag.ShopId = new SelectList(_context.Shops, "Id", "Name", category.ShopId);
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.ShopCategories
                .Include(c => c.Shop)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.ShopCategories.FindAsync(id);
            if (category != null)
            {
                var shopId = category.ShopId;
                _context.ShopCategories.Remove(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { shopId });
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.ShopCategories.Any(e => e.Id == id);
        }
    }
}
