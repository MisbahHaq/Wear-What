using Microsoft.EntityFrameworkCore;
using Nexora.Models;

namespace Nexora.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }
    }
}