using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using MultiVendor.Application.Orders.Queries;

namespace MultiVendor.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetMyOrders([FromQuery] GetMyOrdersQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetOrder(Guid id)
    {
        return Ok(new { message = $"Get order details: {id}" });
    }
}
