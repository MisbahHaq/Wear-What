using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("Shops");
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Slug).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.OwnerId);
        builder.HasIndex(s => s.ApprovalStatus);
    }
}
