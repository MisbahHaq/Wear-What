using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultiVendor.Application.Categories.Queries;

namespace MultiVendor.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetCategories([FromQuery] GetCategoriesQuery query)
    {
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("tree")]
    public async Task<ActionResult> GetCategoryTree()
    {
        return Ok(new { message = "Get category tree" });
    }
}
