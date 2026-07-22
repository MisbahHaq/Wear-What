using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiVendor.Core;
using MultiVendor.Core.Data;

namespace MultiVendor.Backoffice;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new AuthorizeFilter());
        });

        builder.Services.AddMultiVendorIdentity(builder.Configuration);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromHours(1);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Admin/Account/Login";
            options.LogoutPath = "/Admin/Account/Login";
            options.AccessDeniedPath = "/Admin/Account/AccessDenied";
        });

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            await DbInitializer.InitializeAsync(scope.ServiceProvider);
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run("http://localhost:5001");
    }
}
