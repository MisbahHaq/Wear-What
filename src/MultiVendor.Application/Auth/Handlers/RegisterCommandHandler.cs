using MediatR;
using MultiVendor.Application.Auth.DTOs;
using MultiVendor.Application.Common;
using MultiVendor.Application.Interfaces;

namespace MultiVendor.Application.Auth.Handlers;

public class RegisterCommandHandler(IAuthService authService) : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await authService.RegisterAsync(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.Role ?? "Customer",
            cancellationToken);
    }
}
