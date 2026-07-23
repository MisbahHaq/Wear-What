using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MultiVendor.Application.Common;
using MultiVendor.Application.Interfaces;
using MediatR;

namespace MultiVendor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped<IUserContext, UserContext>();
        return services;
    }
}
