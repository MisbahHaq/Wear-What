using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultiVendor.Application.Products.Queries;

namespace MultiVendor.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetProduct(Guid id)
    {
        return Ok(new { message = $"Get product details: {id}" });
    }
}
