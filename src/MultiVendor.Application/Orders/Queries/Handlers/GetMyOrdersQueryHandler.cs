using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Application.Common;
using MultiVendor.Domain.Entities;
using MultiVendor.Domain.Common;
using MultiVendor.Application.Interfaces;

namespace MultiVendor.Application.Orders.Queries.Handlers;

public class GetMyOrdersQueryHandler(IUnitOfWork unitOfWork, IUserContext userContext) : IRequestHandler<GetMyOrdersQuery, Result<List<Order>>>
{
    public async Task<Result<List<Order>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var repo = unitOfWork.Repository<Order>();
        var orders = await repo.GetQueryable()
            .Where(o => o.CustomerId == userContext.UserId.ToString())
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<List<Order>>.Success(orders);
    }
}
