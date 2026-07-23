using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.Property(p => p.Status).HasConversion<int>().IsRequired();
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(10).IsRequired();
        builder.Property(p => p.PaymentMethod).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.TransactionId);
    }
}
