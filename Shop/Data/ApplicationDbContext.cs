using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Shop.Models;

namespace Shop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ShopItem> Shops { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShopCategory> ShopCategories { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<ShopFollow> ShopFollows { get; set; }
        public DbSet<ProductComment> ProductComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<WishlistItem>()
                .HasKey(w => new { w.UserId, w.ProductId });

            builder.Entity<WishlistItem>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ShopFollow>()
                .HasKey(f => new { f.UserId, f.ShopId });

            builder.Entity<ShopFollow>()
                .HasOne(f => f.Shop)
                .WithMany()
                .HasForeignKey(f => f.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductComment>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ProductId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ShopCategory>()
                .HasOne(c => c.Shop)
                .WithMany()
                .HasForeignKey(c => c.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}