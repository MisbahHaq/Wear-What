using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MultiVendor.DTOs;

namespace MultiVendor.ViewComponents;

public class CartCountViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var json = HttpContext.Session.GetString("cart");
        var items = string.IsNullOrWhiteSpace(json)
            ? new List<CartItemDto>()
            : JsonSerializer.Deserialize<List<CartItemDto>>(json) ?? new List<CartItemDto>();

        return View(items.Sum(i => i.Quantity));
    }
}
