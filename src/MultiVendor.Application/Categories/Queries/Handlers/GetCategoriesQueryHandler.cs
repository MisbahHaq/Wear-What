using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Application.Common;
using MultiVendor.Application.Categories.DTOs;
using MultiVendor.Domain.Common;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Application.Categories.Queries.Handlers;

public class GetCategoriesQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var repo = unitOfWork.Repository<Category>();
        var query = repo.GetQueryable().AsQueryable();

        if (request.ActiveOnly)
            query = query.Where(c => c.IsActive);

        if (request.ParentId.HasValue)
            query = query.Where(c => c.ParentId == request.ParentId.Value);

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                ParentId = c.ParentId,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<List<CategoryDto>>.Success(categories);
    }
}
