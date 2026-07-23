using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.Property(r => r.Title).HasMaxLength(300);
        builder.Property(r => r.Comment).HasMaxLength(2000);
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => new { r.ProductId, r.CustomerId }).IsUnique();
        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
