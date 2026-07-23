using Microsoft.AspNetCore.Mvc;
using MediatR;
using MultiVendor.Application.Auth.DTOs;
using MultiVendor.Application.Auth;

namespace MultiVendor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCommand command)
    {
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await mediator.Send(command);
        if (!result.IsSuccess)
            return Unauthorized(result.Error);

        return Ok(result.Data);
    }
}
