namespace MultiVendor.ViewModels;

public class HomePageViewModel
{
    public List<MultiVendor.DTOs.ShopDto> FeaturedShops { get; set; } = new();
    public List<MultiVendor.DTOs.ProductDto> NewArrivals { get; set; } = new();
}
