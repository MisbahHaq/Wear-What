using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.DiscountedUnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");
        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ShopId);
        builder.HasOne(oi => oi.Shop)
            .WithMany(s => s.OrderItems)
            .HasForeignKey(oi => oi.ShopId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
