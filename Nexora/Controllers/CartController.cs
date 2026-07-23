using Microsoft.AspNetCore.Mvc;
using Nexora.Data;
using Nexora.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Nexora.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var cartViewItems = await GetCartItemsAsync();
        var cartDto = BuildCartDto(cartViewItems);
        return Ok(cartDto);
    }

    [HttpPost("add")]
    public async Task<ActionResult<CartDto>> Add([FromBody] AddToCartDto request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { message = "Quantity must be greater than 0." });
        }

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            return NotFound(new { message = "Product not found." });
        }

        var normalizedSize = NormalizeSize(request.Size);
        var normalizedColor = NormalizeColor(request.Color);

        var cart = LoadCart();
        var existingCartItem = cart.FirstOrDefault(item => item.ProductId == request.ProductId && item.Size == normalizedSize && item.Color == normalizedColor);
        var currentQuantity = existingCartItem?.Quantity ?? 0;

        if (currentQuantity + request.Quantity > product.Stock)
        {
            return BadRequest(new { message = $"Only {product.Stock} items available in stock." });
        }

        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (!string.IsNullOrEmpty(userEmail))
        {
            var dbCartItem = normalizedSize == string.Empty && normalizedColor == string.Empty
                ? await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == request.ProductId && (c.Size == null || c.Size == normalizedSize) && (c.Color == null || c.Color == normalizedColor))
                : await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == request.ProductId && c.Size == normalizedSize && c.Color == normalizedColor);

            if (dbCartItem != null)
            {
                dbCartItem.Quantity += request.Quantity;
            }
            else
            {
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    UserEmail = userEmail,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Size = normalizedSize == string.Empty ? null : normalizedSize,
                    Color = normalizedColor == string.Empty ? null : normalizedColor
                });
            }
            await _context.SaveChangesAsync();
        }

        if (existingCartItem != null)
        {
            existingCartItem.Quantity += request.Quantity;
        }
        else
        {
            cart.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity, Size = normalizedSize, Color = normalizedColor });
        }
        SaveCart(cart);

        var result = await GetCartItemsAsync();
        return Ok(BuildCartDto(result));
    }

    [HttpPut("update")]
    public async Task<ActionResult<CartDto>> Update([FromBody] UpdateCartItemDto request)
    {
        if (request.Quantity < 0)
        {
            return BadRequest(new { message = "Quantity cannot be negative." });
        }

        var key = request.CartItemId;
        if (string.IsNullOrEmpty(key))
        {
            return BadRequest(new { message = "CartItemId is required." });
        }

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isDbItem = key.StartsWith("db:", StringComparison.Ordinal);

        if (isDbItem)
        {
            var idPart = key.Substring(3);
            if (!int.TryParse(idPart, out var dbId))
            {
                return BadRequest(new { message = "Invalid cart item ID." });
            }

            var dbCartItem = await _context.ShoppingCarts.FindAsync(dbId);
            if (dbCartItem == null)
            {
                return NotFound(new { message = "Cart item not found." });
            }

            if (!string.IsNullOrEmpty(userEmail) && dbCartItem.UserEmail != userEmail)
            {
                return Unauthorized();
            }

            if (request.Quantity == 0)
            {
                _context.ShoppingCarts.Remove(dbCartItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                var product = await _context.Products.FindAsync(dbCartItem.ProductId);
                if (product != null && request.Quantity > product.Stock)
                {
                    return BadRequest(new { message = $"Only {product.Stock} items available in stock." });
                }

                dbCartItem.Quantity = request.Quantity;
                await _context.SaveChangesAsync();
            }
        }
        else
        {
            var cart = LoadCart();
            var cartItem = FindSessionCartItemByKey(cart, key);
            if (cartItem == null)
            {
                return NotFound(new { message = "Cart item not found." });
            }

            if (request.Quantity == 0)
            {
                cart.Remove(cartItem);
            }
            else
            {
                var product = await _context.Products.FindAsync(cartItem.ProductId);
                if (product != null && request.Quantity > product.Stock)
                {
                    return BadRequest(new { message = $"Only {product.Stock} items available in stock." });
                }

                cartItem.Quantity = request.Quantity;
            }
            SaveCart(cart);
        }

        var result = await GetCartItemsAsync();
        return Ok(BuildCartDto(result));
    }

    [HttpDelete("{cartItemId}")]
    public async Task<ActionResult<CartDto>> Remove(string cartItemId)
    {
        if (string.IsNullOrEmpty(cartItemId))
        {
            return BadRequest(new { message = "CartItemId is required." });
        }

        var userEmail = HttpContext.Session.GetString("UserEmail");
        var isDbItem = cartItemId.StartsWith("db:", StringComparison.Ordinal);

        if (isDbItem)
        {
            var idPart = cartItemId.Substring(3);
            if (!int.TryParse(idPart, out var dbId))
            {
                return BadRequest(new { message = "Invalid cart item ID." });
            }

            var dbCartItem = await _context.ShoppingCarts.FindAsync(dbId);
            if (dbCartItem == null)
            {
                return NotFound(new { message = "Cart item not found." });
            }

            if (!string.IsNullOrEmpty(userEmail) && dbCartItem.UserEmail != userEmail)
            {
                return Unauthorized();
            }

            _context.ShoppingCarts.Remove(dbCartItem);
            await _context.SaveChangesAsync();
        }
        else
        {
            var cart = LoadCart();
            var cartItem = FindSessionCartItemByKey(cart, cartItemId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
            }
        }

        var result = await GetCartItemsAsync();
        return Ok(BuildCartDto(result));
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> Clear()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(userEmail))
        {
            var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == userEmail);
            _context.ShoppingCarts.RemoveRange(dbCartItems);
            await _context.SaveChangesAsync();
        }

        HttpContext.Session.Remove("Cart");

        return Ok(BuildCartDto(new List<CartViewModel.CartItemViewModel>()));
    }

    [HttpPost("merge")]
    public async Task<ActionResult<CartDto>> Merge()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return BadRequest(new { message = "User must be logged in to merge cart." });
        }

        var sessionCart = LoadCart();
        if (!sessionCart.Any())
        {
            var dbCartItems = await GetCartItemsAsync();
            return Ok(BuildCartDto(dbCartItems));
        }

        foreach (var sessionItem in sessionCart)
        {
            var normalizedSize = sessionItem.Size;
            var normalizedColor = sessionItem.Color;

            var dbCartItem = normalizedSize == string.Empty && normalizedColor == string.Empty
                ? await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == sessionItem.ProductId && (c.Size == null || c.Size == normalizedSize) && (c.Color == null || c.Color == normalizedColor))
                : await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == sessionItem.ProductId && c.Size == normalizedSize && c.Color == normalizedColor);

            if (dbCartItem != null)
            {
                dbCartItem.Quantity += sessionItem.Quantity;
            }
            else
            {
                _context.ShoppingCarts.Add(new ShoppingCart
                {
                    UserEmail = userEmail,
                    ProductId = sessionItem.ProductId,
                    Quantity = sessionItem.Quantity,
                    Size = normalizedSize == string.Empty ? null : normalizedSize,
                    Color = normalizedColor == string.Empty ? null : normalizedColor
                });
            }
        }

        await _context.SaveChangesAsync();

        HttpContext.Session.Remove("Cart");

        var result = await GetCartItemsAsync();
        return Ok(BuildCartDto(result));
    }

    private List<CartItem> LoadCart()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");

        if (!string.IsNullOrEmpty(userEmail))
        {
            var dbCartItems = _context.ShoppingCarts.Where(c => c.UserEmail == userEmail).ToList();

            var sessionCartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(sessionCartJson))
            {
                var mergedCart = dbCartItems.Select(c => new CartItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Size = c.Size ?? string.Empty,
                    Color = c.Color ?? string.Empty
                }).ToList();

                var cartJsonToSave = JsonSerializer.Serialize(mergedCart);
                HttpContext.Session.SetString("Cart", cartJsonToSave);
                return mergedCart;
            }
        }

        var cartJson = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cartJson))
        {
            return new List<CartItem>();
        }

        return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
    }

    private void SaveCart(List<CartItem> cart)
    {
        var cartJson = JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString("Cart", cartJson);
    }

    private static string NormalizeSize(string? size)
    {
        return (size ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string NormalizeColor(string? color)
    {
        return (color ?? string.Empty).Trim();
    }

    private async Task<List<CartViewModel.CartItemViewModel>> GetCartItemsAsync()
    {
        var cart = LoadCart();
        var productIds = cart.Select(item => item.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var cartItems = new List<CartViewModel.CartItemViewModel>();
        foreach (var cartItem in cart)
        {
            var product = products.FirstOrDefault(p => p.Id == cartItem.ProductId);
            if (product != null)
            {
                cartItems.Add(new CartViewModel.CartItemViewModel
                {
                    Product = product,
                    Quantity = cartItem.Quantity,
                    Size = cartItem.Size ?? string.Empty,
                    Color = cartItem.Color ?? string.Empty
                });
            }
        }

        return cartItems;
    }

    private async Task<CartDto> BuildCartDtoAsync(List<CartViewModel.CartItemViewModel> cartViewItems)
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var dbCartIdLookup = new Dictionary<(int ProductId, string Size, string Color), int>();

        if (!string.IsNullOrEmpty(userEmail) && cartViewItems.Any())
        {
            var distinctKeys = cartViewItems
                .Select(i => (ProductId: i.Product!.Id, Size: NormalizeSize(i.Size), Color: NormalizeColor(i.Color)))
                .Distinct()
                .ToList();

            foreach (var key in distinctKeys)
            {
                ShoppingCart? dbItem = null;
                if (key.Size == string.Empty && key.Color == string.Empty)
                {
                    dbItem = await _context.ShoppingCarts
                        .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == key.ProductId && (c.Size == null || c.Size == key.Size) && (c.Color == null || c.Color == key.Color));
                }
                else
                {
                    dbItem = await _context.ShoppingCarts
                        .FirstOrDefaultAsync(c => c.UserEmail == userEmail && c.ProductId == key.ProductId && c.Size == key.Size && c.Color == key.Color);
                }

                if (dbItem != null)
                {
                    dbCartIdLookup[key] = dbItem.Id;
                }
            }
        }

        var dtoItems = cartViewItems
            .Where(i => i.Product != null)
            .Select(item =>
            {
                var sizeKey = NormalizeSize(item.Size);
                var colorKey = NormalizeColor(item.Color);
                var productKey = (item.Product!.Id, sizeKey, colorKey);

                var cartItemId = dbCartIdLookup.TryGetValue(productKey, out var dbId)
                    ? $"db:{dbId}"
                    : $"sess:{item.Product.Id}:{sizeKey}:{colorKey}";

                return new CartItemDto
                {
                    CartItemId = cartItemId,
                    ProductId = item.Product.Id,
                    ProductName = item.Product.Name,
                    ProductImage = item.Product.ImageUrl,
                    UnitPrice = item.Product.Price,
                    Quantity = item.Quantity,
                    Size = item.Size,
                    Color = item.Color,
                    LineTotal = item.Product.Price * item.Quantity
                };
            })
            .ToList();

        return new CartDto
        {
            Items = dtoItems,
            TotalAmount = dtoItems.Sum(i => i.LineTotal),
            ItemCount = dtoItems.Sum(i => i.Quantity)
        };
    }

    private CartItem? FindSessionCartItemByKey(List<CartItem> cart, string key)
    {
        if (!key.StartsWith("sess:", StringComparison.Ordinal)) return null;
        var payload = key.Substring(5);
        var parts = payload.Split(':', 3);
        if (parts.Length != 3) return null;

        if (!int.TryParse(parts[0], out var productId)) return null;

        return cart.FirstOrDefault(item => item.ProductId == productId && item.Size == parts[1] && item.Color == parts[2]);
    }

    private CartDto BuildCartDto(List<CartViewModel.CartItemViewModel> cartViewItems)
    {
        return BuildCartDtoAsync(cartViewItems).GetAwaiter().GetResult();
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public class CartItemViewModel
        {
            public Product? Product { get; set; }
            public int Quantity { get; set; }
            public string Size { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public decimal Subtotal => Product?.Price * Quantity ?? 0;
        }
    }
}
