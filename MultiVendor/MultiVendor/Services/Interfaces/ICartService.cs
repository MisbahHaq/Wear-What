using MultiVendor.ViewModels;

namespace MultiVendor.Services.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();
    Task AddItemAsync(int productId, int quantity);
    Task UpdateQuantityAsync(int productId, int quantity);
    Task RemoveItemAsync(int productId);
    Task ClearAsync();
}
