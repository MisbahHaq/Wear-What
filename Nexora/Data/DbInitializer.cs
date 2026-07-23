using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nexora.Models;

namespace Nexora.Data
{
    public static class DbInitializer
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            // Only seed if no products exist
            if (await context.Products.AnyAsync()) return;

            var productsPath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");
            if (!File.Exists(productsPath))
            {
                var fallbackPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "products.json"));
                if (File.Exists(fallbackPath))
                    productsPath = fallbackPath;
            }

            if (!File.Exists(productsPath))
                return;

            var json = await File.ReadAllTextAsync(productsPath);
            var products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions);

            if (products is not null && products.Any())
            {
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
            else
            {
                var fallbackProducts = new List<Product>
                {
                    new Product { Name = "Represent T-Shirt", Description = "Essential cotton tee", Price = 29.99m, Stock = 100, Gender = "Men", ImageUrl = "/assets/tshirt1.jpg", Tags = "new,bestseller", Colors = "Black,White,Grey" },
                    new Product { Name = "Represent Hoodie", Description = " heavyweight fleece", Price = 89.99m, Stock = 50, Gender = "Men", ImageUrl = "/assets/hoodie1.jpg", Tags = "new", Colors = "Black,Navy" },
                    new Product { Name = "Represent Cap", Description = "Embroidered logo cap", Price = 34.99m, Stock = 75, Gender = "Women", ImageUrl = "/images/cap.jpg", Tags = "accessories", Colors = "Black,White" },
                    new Product { Name = "Represent Jeans", Description = "Slim fit denim", Price = 99.99m, Stock = 40, Gender = "Women", ImageUrl = "/images/jeans.jpg", Tags = "bestseller", Colors = "Indigo,Black" }
                };

                await context.Products.AddRangeAsync(fallbackProducts);
                await context.SaveChangesAsync();
            }
        }
    }
}
