using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Models;

namespace MultiVendor.Data.Configurations;

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.OwnerId);
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(s => s.Owner)
            .WithMany(u => u.Shops)
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Products)
            .WithOne(p => p.Shop)
            .HasForeignKey(p => p.ShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Followers)
            .WithOne(f => f.Shop)
            .HasForeignKey(f => f.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
