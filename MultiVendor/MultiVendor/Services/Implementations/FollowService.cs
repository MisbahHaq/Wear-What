using Microsoft.EntityFrameworkCore;
using MultiVendor.Data;
using MultiVendor.DTOs;
using MultiVendor.Models;
using MultiVendor.Services.Interfaces;

namespace MultiVendor.Services.Implementations;

public class FollowService : IFollowService
{
    private readonly ECommerceDbContext _context;

    public FollowService(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> FollowShopAsync(string userId, int shopId)
    {
        var exists = await _context.Follows
            .AnyAsync(f => f.FollowerId == userId && f.ShopId == shopId);

        if (exists) return false;

        var follow = new Follow
        {
            FollowerId = userId,
            ShopId = shopId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Follows.Add(follow);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnfollowShopAsync(string userId, int shopId)
    {
        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == userId && f.ShopId == shopId);

        if (follow is null) return false;

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ProductDto>> GetFollowedShopsFeedAsync(
        string userId, int page = 1, int pageSize = 20)
    {
        var skip = (page - 1) * pageSize;

        var products = await _context.Follows
            .Where(f => f.FollowerId == userId)
            .SelectMany(f => f.Shop.Products
                .Where(p => p.IsActive)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    ShopId = p.ShopId,
                    ShopName = p.Shop.Name,
                    Name = p.Name,
                    ShortDescription = p.ShortDescription,
                    Price = p.Price,
                    Stock = p.Stock,
                    CoverImageUrl = p.CoverImageUrl,
                    Category = p.Category,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                }))
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return products;
    }

    public async Task<int> GetFollowersCountAsync(int shopId)
    {
        return await _context.Follows.CountAsync(f => f.ShopId == shopId);
    }

    public async Task<bool> IsFollowingAsync(string userId, int shopId)
    {
        return await _context.Follows
            .AnyAsync(f => f.FollowerId == userId && f.ShopId == shopId);
    }
}
