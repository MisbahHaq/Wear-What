using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Authorize]
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }

        public async Task<IActionResult> Create(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var model = new ProductComment { ProductId = productId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductComment model)
        {
            if (ModelState.IsValid)
            {
                model.UserId = GetCurrentUserId();
                model.CreatedAt = DateTime.UtcNow;
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.ProductComments.FindAsync(id);
            if (comment == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (comment.UserId != currentUserId)
            {
                return Forbid();
            }

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductComment model)
        {
            if (id != model.Id) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (model.UserId != currentUserId)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Attach(model).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(model.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.ProductComments
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null) return NotFound();

            var currentUserId = GetCurrentUserId();
            if (comment.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.ProductComments.FindAsync(id);
            if (comment != null)
            {
                _context.ProductComments.Remove(comment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Products", new { id = comment?.ProductId ?? 0 });
        }

        private bool CommentExists(int id)
        {
            return _context.ProductComments.Any(e => e.Id == id);
        }
    }
}