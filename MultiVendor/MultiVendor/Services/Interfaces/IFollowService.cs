using MultiVendor.DTOs;
using MultiVendor.Models;

namespace MultiVendor.Services.Interfaces;

public interface IFollowService
{
    Task<bool> FollowShopAsync(string userId, int shopId);
    Task<bool> UnfollowShopAsync(string userId, int shopId);
    Task<IEnumerable<ProductDto>> GetFollowedShopsFeedAsync(string userId, int page = 1, int pageSize = 20);
    Task<int> GetFollowersCountAsync(int shopId);
    Task<bool> IsFollowingAsync(string userId, int shopId);
}
