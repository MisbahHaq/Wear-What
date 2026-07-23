using MediatR;
using MultiVendor.Application.Common;
using MultiVendor.Application.Products.DTOs;

namespace MultiVendor.Application.Products.Queries;

public class GetProductsQuery : IBaseQuery<List<ProductDto>>
{
    public Guid? CategoryId { get; set; }
    public Guid? ShopId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
