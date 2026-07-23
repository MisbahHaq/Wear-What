using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Application.Common;
using MultiVendor.Application.Products.DTOs;
using MultiVendor.Domain.Entities;
using MultiVendor.Domain.Common;

namespace MultiVendor.Application.Products.Queries.Handlers;

public class GetProductsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var repo = unitOfWork.Repository<Product>();
        var query = repo.GetQueryable()
            .Include(p => p.Shop)
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.ShopId.HasValue)
            query = query.Where(p => p.ShopId == request.ShopId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p => p.Name.Contains(request.SearchTerm));

        query = request.SortBy?.ToLower() switch
        {
            "price" => request.SortDescending ? query.OrderByDescending(p => p.BasePrice) : query.OrderBy(p => p.BasePrice),
            "rating" => request.SortDescending ? query.OrderByDescending(p => p.AverageRating) : query.OrderBy(p => p.AverageRating),
            "sales" => request.SortDescending ? query.OrderByDescending(p => p.SalesCount) : query.OrderBy(p => p.SalesCount),
            _ => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
        };

        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                BasePrice = p.BasePrice,
                MainImageUrl = p.MainImageUrl,
                StockQuantity = p.StockQuantity,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                ShopName = p.Shop.Name,
                CategoryName = p.Category.Name
            })
            .ToListAsync(cancellationToken);

        return Result<List<ProductDto>>.Success(products);
    }
}
