using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using System.Text.Json;

namespace Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson) ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(cartJson)!;
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        private int GetCartCount()
        {
            return GetCart().Sum(i => i.Quantity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            using var scope = HttpContext.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = await db.Products
                .Include(p => p.Shop)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.ProductId == productId);
            var currentQty = existing?.Quantity ?? 0;

            if (product.StockQuantity <= 0)
            {
                TempData["CartMessage"] = $"{product.Name} is out of stock.";
                var referer0 = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer0) && Url.IsLocalUrl(referer0))
                    return LocalRedirect(referer0);
                return RedirectToAction("Index", "Products");
            }

            if (currentQty + quantity > product.StockQuantity)
            {
                TempData["CartMessage"] = $"Only {product.StockQuantity} of {product.Name} available.";
                quantity = product.StockQuantity - currentQty;
                if (quantity <= 0)
                {
                    var refererX = Request.Headers["Referer"].ToString();
                    if (!string.IsNullOrEmpty(refererX) && Url.IsLocalUrl(refererX))
                        return LocalRedirect(refererX);
                    return RedirectToAction("Index", "Products");
                }
            }

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl1 ?? string.Empty,
                    ShopId = product.ShopId,
                    ShopName = product.Shop?.Name ?? string.Empty
                });
            }

            SaveCart(cart);
            TempData["CartMessage"] = $"{product.Name} added to cart.";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
                return LocalRedirect(referer);

            return RedirectToAction("Index", "Products");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null) cart.Remove(item);
            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0) cart.Remove(item);
                else item.Quantity = quantity;
            }
            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpGet]
        public IActionResult Count()
        {
            var count = GetCartCount();
            return Json(new { count });
        }
    }
}
