using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.Models;

namespace Shop.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            await context.Database.MigrateAsync();
            logger.LogInformation("Database migration completed");

            try
            {
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                    logger.LogInformation("Created Admin role");
                }

                if (!await roleManager.RoleExistsAsync("Vendor"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Vendor"));
                    logger.LogInformation("Created Vendor role");
                }

                var adminEmail = "admin@shop.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "Admin",
                        EmailConfirmed = true
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        logger.LogInformation("Created default admin user with email={Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogWarning("Failed to create default admin user. Errors: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger.LogInformation("Default admin user already exists with email={Email}", adminEmail);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DbInitializer error: {Message}", ex.Message);
            }
        }
    }
}