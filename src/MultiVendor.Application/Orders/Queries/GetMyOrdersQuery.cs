using MediatR;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Application.Common;
using MultiVendor.Domain.Entities;
using MultiVendor.Domain.Common;

namespace MultiVendor.Application.Orders.Queries;

public class GetMyOrdersQuery : IBaseQuery<List<Order>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
