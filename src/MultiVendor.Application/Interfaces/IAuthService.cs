using MultiVendor.Application.Auth.DTOs;
using MultiVendor.Application.Common;

namespace MultiVendor.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(string email, string password, string firstName, string lastName, string role, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> LoginAsync(string email, string password, CancellationToken cancellationToken);
}
