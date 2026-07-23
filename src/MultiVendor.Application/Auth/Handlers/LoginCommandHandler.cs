using MediatR;
using MultiVendor.Application.Auth.DTOs;
using MultiVendor.Application.Common;
using MultiVendor.Application.Interfaces;

namespace MultiVendor.Application.Auth.Handlers;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
