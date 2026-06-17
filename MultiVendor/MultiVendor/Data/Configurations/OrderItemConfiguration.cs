using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Models;

namespace MultiVendor.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasIndex(oi => new { oi.ShopOrderId, oi.ProductId });
        builder.HasIndex(oi => oi.ProductId);

        builder.HasOne(oi => oi.ShopOrder)
            .WithMany(so => so.OrderItems)
            .HasForeignKey(oi => oi.ShopOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
