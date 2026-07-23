using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.Property(pv => pv.Name).HasMaxLength(500).IsRequired();
        builder.Property(pv => pv.Price).HasColumnType("decimal(18,2)");
        builder.Property(pv => pv.Sku).HasMaxLength(100);
        builder.HasIndex(pv => pv.ProductId);
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
