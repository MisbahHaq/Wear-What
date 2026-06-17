using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiVendor.Models;

namespace MultiVendor.Data.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.HasIndex(f => new { f.FollowerId, f.ShopId }).IsUnique();
        builder.HasIndex(f => f.ShopId);

        builder.HasOne(f => f.Follower)
            .WithMany(u => u.Follows)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Shop)
            .WithMany(s => s.Followers)
            .HasForeignKey(f => f.ShopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
