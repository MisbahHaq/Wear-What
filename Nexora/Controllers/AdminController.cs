using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;

namespace Nexora.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Email == "admin@represent.com" && model.Password == "Qwerty123")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    HttpContext.Session.SetString("AdminEmail", model.Email);
                    return RedirectToAction("Dashboard", "Admin");
                }
                ModelState.AddModelError("", "Invalid admin credentials");
            }
            return RedirectToAction("Login", "Account");
        }

        // POST: Admin/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            HttpContext.Session.Remove("AdminEmail");
            HttpContext.Session.Remove("UserEmail");
            HttpContext.Session.Remove("AuthToken");
            return RedirectToAction("Login", "Account");
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            var products = await _context.Products.Take(10).ToListAsync();

            // Calculate best selling product
            var bestSellingProduct = await _context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQuantity = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefaultAsync();

            Product? bestSellingProductInfo = null;
            int bestSellingProductQuantity = 0;

            if (bestSellingProduct != null)
            {
                bestSellingProductInfo = await _context.Products.FindAsync(bestSellingProduct.ProductId);
                bestSellingProductQuantity = bestSellingProduct.TotalQuantity;
            }

            // Calculate most bookmarked product
            var mostBookmarkedProduct = await _context.Bookmarks
                .GroupBy(b => b.ProductId)
                .Select(g => new { ProductId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            Product? mostBookmarkedProductInfo = null;
            int mostBookmarkedProductCount = 0;

            if (mostBookmarkedProduct != null)
            {
                mostBookmarkedProductInfo = await _context.Products.FindAsync(mostBookmarkedProduct.ProductId);
                mostBookmarkedProductCount = mostBookmarkedProduct.Count;
            }

            var supportRequests = await _context.SupportRequests
                .Include(r => r.Order)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending"),
                OrderReceivedOrders = await _context.Orders.CountAsync(o => o.Status == "Order Received"),
                OutForDeliveryOrders = await _context.Orders.CountAsync(o => o.Status == "Out for Delivery"),
                PendingCancellationRequests = await _context.SupportRequests.CountAsync(r => r.RequestType == "Cancellation" && r.Status != "Resolved"),
                NewChatRequests = await _context.SupportRequests.CountAsync(r => r.RequestType == "AdminChat" && !r.IsRead),
                UnreadSupportRequests = await _context.SupportRequests.CountAsync(r => !r.IsRead),
                RecentOrders = orders,
                RecentProducts = products,
                BestSellingProduct = bestSellingProductInfo,
                BestSellingProductQuantity = bestSellingProductQuantity,
                MostBookmarkedProduct = mostBookmarkedProductInfo,
                MostBookmarkedProductCount = mostBookmarkedProductCount,
                RecentSupportRequests = supportRequests
            };

            return View(model);
        }

        // GET: Admin/Products
        public async Task<IActionResult> Products()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: Admin/Products/Create
        public IActionResult CreateProduct()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (ModelState.IsValid)
            {
                product.ImageUrls = NormalizeImageUrls(product.ImageUrls, Request.Form);
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Products));
            }
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> EditProduct(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    product.ImageUrls = NormalizeImageUrls(product.ImageUrls, Request.Form);
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Products));
            }
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> DeleteProduct(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var orders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // GET: Admin/SupportRequests
        public async Task<IActionResult> SupportRequests()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var requests = await _context.SupportRequests
                .Include(r => r.Order)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: Admin/Cancellations
        public async Task<IActionResult> Cancellations()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var requests = await _context.SupportRequests
                .Include(r => r.Order)
                .Where(r => r.RequestType == "Cancellation")
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        // GET: Admin/UnreadSupport
        public async Task<IActionResult> UnreadSupport()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            var requests = await _context.SupportRequests
                .Include(r => r.Order)
                .Where(r => !r.IsRead)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> MarkSupportRequestRead([FromBody] SupportRequestActionModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var request = await _context.SupportRequests.FindAsync(model?.Id ?? 0);
            if (request == null)
            {
                return Json(new { success = false, message = "Request not found" });
            }

            request.IsRead = true;
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Request marked as read" });
        }

        [HttpPost]
        public async Task<IActionResult> ResolveSupportRequest([FromBody] SupportRequestActionModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var request = await _context.SupportRequests.FindAsync(model?.Id ?? 0);
            if (request == null)
            {
                return Json(new { success = false, message = "Request not found" });
            }

            request.AdminResponse = string.IsNullOrWhiteSpace(model?.Response) ? request.AdminResponse : model.Response;
            request.Status = "Resolved";
            request.IsRead = true;
            request.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Request resolved" });
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrderFromChat([FromBody] SupportRequestActionModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var requestId = model?.Id ?? 0;
            var request = await _context.SupportRequests
                .Include(r => r.Order)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                return Json(new { success = false, message = "Request not found" });
            }

            if (request.Order != null)
            {
                request.Order.Status = "Cancelled";
            }

            request.Status = "Resolved";
            request.IsRead = true;
            request.UpdatedAt = DateTime.UtcNow;
            request.AdminResponse = "Order cancelled by admin from chatbot request.";
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Order cancelled" });
        }

        // POST: Admin/Orders/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderStatusUpdateModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var order = await _context.Orders.FindAsync(model.OrderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            var validStatuses = new[] { "Pending", "Order Received", "Out for Delivery", "Delivered", "Cancelled" };
            if (!validStatuses.Contains(model.Status))
            {
                return Json(new { success = false, message = "Invalid status" });
            }

            order.Status = model.Status;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status updated" });
        }

        // GET: Admin/OrderDetail/5
        public async Task<IActionResult> OrderDetail(int? id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        private static string? NormalizeImageUrls(string? existingImageUrls, IFormCollection form)
        {
            var images = new List<string>();
            if (!string.IsNullOrWhiteSpace(existingImageUrls))
            {
                images.AddRange(existingImageUrls.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(url => url.Trim()));
            }

            for (var i = 0; i < 5; i++)
            {
                var value = form[$"ImageUrls[{i}]"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    images.Add(value.Trim());
                }
            }

            return images.Any() ? string.Join(", ", images.Distinct(StringComparer.OrdinalIgnoreCase)) : existingImageUrls;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}

public class SupportRequestActionModel
{
    public int Id { get; set; }
    public string? Response { get; set; }
}

public class OrderStatusUpdateModel
{
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
}