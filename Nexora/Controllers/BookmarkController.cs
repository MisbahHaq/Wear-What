using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using System.Collections.Generic;
using System.Linq;

namespace Nexora.Controllers
{
    public class BookmarkController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookmarkController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookmark
        public async Task<IActionResult> Index()
        {
            var bookmarkedItems = await GetBookmarkedItemsAsync();
            var viewModel = new BookmarkViewModel
            {
                Items = bookmarkedItems
            };
            return View(viewModel);
        }

        // POST: Bookmark/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var email = HttpContext.Session.GetString("UserEmail") ?? HttpContext.Session.GetString("AdminEmail");

            // Add to bookmarks (stored in session and database)
            var bookmarks = GetBookmarks();
            if (!bookmarks.Contains(id))
            {
                bookmarks.Add(id);
                SaveBookmarks(bookmarks);

                // Also save to database for analytics (if user is signed in)
                if (!string.IsNullOrEmpty(email))
                {
                    var existingBookmark = await _context.Bookmarks
                        .FirstOrDefaultAsync(b => b.ProductId == id && b.UserEmail == email);
                    if (existingBookmark == null)
                    {
                        _context.Bookmarks.Add(new Bookmark
                        {
                            ProductId = id,
                            UserEmail = email,
                            CreatedAt = DateTime.UtcNow
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Return JSON for AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, bookmarkCount = bookmarks.Count });
            }

            return RedirectToAction("Index");
        }

        // POST: Bookmark/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var bookmarks = GetBookmarks();
            var email = HttpContext.Session.GetString("UserEmail") ?? HttpContext.Session.GetString("AdminEmail");

            if (bookmarks.Contains(id))
            {
                bookmarks.Remove(id);
                SaveBookmarks(bookmarks);

                // Also remove from database
                if (!string.IsNullOrEmpty(email))
                {
                    var bookmark = await _context.Bookmarks
                        .FirstOrDefaultAsync(b => b.ProductId == id && b.UserEmail == email);
                    if (bookmark != null)
                    {
                        _context.Bookmarks.Remove(bookmark);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, bookmarkCount = bookmarks.Count });
            }

            return RedirectToAction("Index");
        }

        // GET: Bookmark/GetCount
        [HttpGet]
        public IActionResult GetCount()
        {
            var bookmarks = GetBookmarks();
            var count = bookmarks.Count;
            return Json(new { count = count });
        }

        private List<int> GetBookmarks()
        {
            var bookmarksJson = HttpContext.Session.GetString("Bookmarks");
            if (string.IsNullOrEmpty(bookmarksJson))
            {
                return new List<int>();
            }

            return System.Text.Json.JsonSerializer.Deserialize<List<int>>(bookmarksJson) ?? new List<int>();
        }

        private void SaveBookmarks(List<int> bookmarks)
        {
            var bookmarksJson = System.Text.Json.JsonSerializer.Serialize(bookmarks);
            HttpContext.Session.SetString("Bookmarks", bookmarksJson);
        }

        private async Task<List<BookmarkItemViewModel>> GetBookmarkedItemsAsync()
        {
            var bookmarkIds = GetBookmarks();
            var products = await _context.Products.Where(p => bookmarkIds.Contains(p.Id)).ToListAsync();

            var bookmarkedItems = new List<BookmarkItemViewModel>();
            foreach (var bookmarkId in bookmarkIds)
            {
                var product = products.FirstOrDefault(p => p.Id == bookmarkId);
                if (product != null)
                {
                    bookmarkedItems.Add(new BookmarkItemViewModel
                    {
                        Product = product
                    });
                }
            }

            return bookmarkedItems;
        }
    }

    public class BookmarkViewModel
    {
        public List<BookmarkItemViewModel> Items { get; set; } = new List<BookmarkItemViewModel>();
    }

    public class BookmarkItemViewModel
    {
        public Product Product { get; set; }
    }
}