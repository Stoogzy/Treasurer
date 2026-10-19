using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Behaviours;

namespace Treasurer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddRequestHandlers();
        services.AddValidation();

        return services;
    }

    private static void AddRequestHandlers(this IServiceCollection services)
    {
        services.Scan(scan => scan.FromAssemblies(typeof(DependencyInjection).Assembly)

            .AddClasses(
                classes => classes
                    .AssignableTo(typeof(IRequestHandler<,>))
                    .Where(type => !type.IsGenericTypeDefinition),
                publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()

            .AddClasses(
                classes => classes
                    .AssignableTo(typeof(IRequestHandler<>))
                    .Where(type => !type.IsGenericTypeDefinition),
                publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }

    private static void AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationHandlerDecorator<,>));

        // Add this only after introducing a non-result IRequestHandler<T>.
        // services.Decorate(typeof(IRequestHandler<>), typeof(ValidationHandlerDecorator<>));
    }
}
