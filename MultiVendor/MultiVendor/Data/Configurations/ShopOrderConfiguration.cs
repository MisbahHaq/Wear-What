using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Models;

namespace MultiVendor.Data.Configurations;

public class ShopOrderConfiguration : IEntityTypeConfiguration<ShopOrder>
{
    public void Configure(EntityTypeBuilder<ShopOrder> builder)
    {
        builder.HasIndex(so => new { so.OrderId, so.ShopId });
        builder.HasIndex(so => so.VendorOrderNumber);
        builder.HasIndex(so => so.Status);
        builder.Property(so => so.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(so => so.Shop)
            .WithMany(s => s.ShopOrders)
            .HasForeignKey(so => so.ShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.Order)
            .WithMany(o => o.ShopOrders)
            .HasForeignKey(so => so.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(so => so.OrderItems)
            .WithOne(oi => oi.ShopOrder)
            .HasForeignKey(oi => oi.ShopOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
