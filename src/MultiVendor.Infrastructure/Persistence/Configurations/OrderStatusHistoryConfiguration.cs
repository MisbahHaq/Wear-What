using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistory");
        builder.Property(osh => osh.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(osh => osh.OrderId);
        builder.Property(osh => osh.Notes).HasMaxLength(1000);
    }
}
