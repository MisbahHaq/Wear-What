using FluentValidation;
using MultiVendor.Application.Common;
using MultiVendor.Application.Auth.DTOs;

namespace MultiVendor.Application.Auth;

public class LoginCommand : IBaseCommand<AuthResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
