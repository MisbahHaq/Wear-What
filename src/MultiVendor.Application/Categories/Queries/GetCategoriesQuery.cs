using MediatR;
using MultiVendor.Application.Categories.DTOs;
using MultiVendor.Application.Common;

namespace MultiVendor.Application.Categories.Queries;

public class GetCategoriesQuery : IBaseQuery<List<CategoryDto>>
{
    public Guid? ParentId { get; set; }
    public bool ActiveOnly { get; set; } = true;
}
