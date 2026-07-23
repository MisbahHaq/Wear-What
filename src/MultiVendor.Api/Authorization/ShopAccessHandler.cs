using ClaimsPrincipalExtensions = MultiVendor.Application.Interfaces.IUserContext;
using Microsoft.AspNetCore.Authorization;

namespace MultiVendor.Api.Authorization;

public class ShopAccessRequirement : IAuthorizationRequirement { }

public class ShopAccessHandler(IAuthorizationHandler next) : AuthorizationHandler<ShopAccessRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ShopAccessRequirement requirement)
    {
        var user = context.User;
        if (user.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userId = user.FindFirst("sub")?.Value;
        var shopId = user.FindFirst("ShopId")?.Value;

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(shopId))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }
}
