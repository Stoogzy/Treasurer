using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Treasurer.Application.Common.AppRequests;
using Treasurer.Application.Common.Behaviours;

namespace Treasurer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        AddRequestHandlers(services);
        AddValidation(services);

        return services;
    }

    private static void AddRequestHandlers(IServiceCollection services)
    {
        //services.Scan(scan =>
        //{
        //    scan.FromAssemblyOf<ListAllPowerPlansQueryHandler>()
        //        .AddClasses(classes => classes
        //            .AssignableTo(typeof(IRequestHandler<,>))
        //            .Where(t => !t.IsGenericTypeDefinition))
        //        .AsImplementedInterfaces()
        //        .WithScopedLifetime();

        //    scan.FromAssemblyOf<ListAllPowerPlansQueryHandler>()
        //        .AddClasses(classes => classes
        //            .AssignableTo(typeof(IRequestHandler<>))
        //            .Where(t => !t.IsGenericTypeDefinition))
        //        .AsImplementedInterfaces()
        //        .WithScopedLifetime();
        //});
    }

    private static void AddValidation(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationHandlerDecorator<,>));
    }
}
