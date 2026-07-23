using Microsoft.AspNetCore.Identity;
using MultiVendor.Application.Interfaces;
using MultiVendor.Application.Common;
using MultiVendor.Domain.Entities;

namespace MultiVendor.Infrastructure.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<Result<MultiVendor.Application.Auth.DTOs.AuthResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string role, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<MultiVendor.Application.Auth.DTOs.AuthResponse>.Failure(errors);
        }

        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new AppRole { Name = role });
        }

        await userManager.AddToRoleAsync(user, role);

        var roles = new List<string> { role };
        var token = jwtTokenService.GenerateToken(user.Id, user.Email, roles);

        var response = new MultiVendor.Application.Auth.DTOs.AuthResponse
        {
            Token = token,
            RefreshToken = string.Empty,
            UserId = Guid.Parse(user.Id),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            ShopId = null
        };

        return Result<MultiVendor.Application.Auth.DTOs.AuthResponse>.Success(response);
    }

    public async Task<Result<MultiVendor.Application.Auth.DTOs.AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result<MultiVendor.Application.Auth.DTOs.AuthResponse>.Failure("Invalid email or password");
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            return Result<MultiVendor.Application.Auth.DTOs.AuthResponse>.Failure("Invalid email or password");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateToken(user.Id, user.Email, roles.ToList());

        var response = new MultiVendor.Application.Auth.DTOs.AuthResponse
        {
            Token = token,
            RefreshToken = string.Empty,
            UserId = Guid.Parse(user.Id),
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToList(),
            ShopId = null
        };

        return Result<MultiVendor.Application.Auth.DTOs.AuthResponse>.Success(response);
    }
}
