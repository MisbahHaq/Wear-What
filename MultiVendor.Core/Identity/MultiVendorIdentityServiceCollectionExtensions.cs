using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MultiVendor.Core.Data;
using MultiVendor.Core.Identity;
using MultiVendor.Core.Models;

namespace MultiVendor.Core;

public static class MultiVendorIdentityServiceCollectionExtensions
{
    public static IServiceCollection AddMultiVendorIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

        return services;
    }
}
