using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Code).HasMaxLength(100);
        builder.Property(d => d.DiscountPercentage).HasColumnType("decimal(5,2)");
        builder.Property(d => d.MaxDiscountAmount).HasColumnType("decimal(18,2)");
        builder.HasIndex(d => d.Code);
        builder.HasIndex(d => d.ShopId);
        builder.HasIndex(d => d.CategoryId);
    }
}
