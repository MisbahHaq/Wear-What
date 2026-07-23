using Microsoft.AspNetCore.Http;
using MultiVendor.Application.Interfaces;

namespace MultiVendor.Application;

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
    public Guid UserId
    {
        get
        {
            var userIdClaim = accessor.HttpContext?.User?.FindFirst("sub")?.Value;
            return Guid.TryParse(userIdClaim, out var guid) ? guid : Guid.Empty;
        }
    }

    public string Email => accessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
    public IList<string> Roles => accessor.HttpContext?.User?.FindAll("role").Select(c => c.Value).ToList() ?? new List<string>();
}
