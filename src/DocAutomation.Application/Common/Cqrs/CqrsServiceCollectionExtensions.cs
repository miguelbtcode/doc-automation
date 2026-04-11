using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DocAutomation.Application.Common.Cqrs;

public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    /// Registra el <see cref="IMediator"/> y escanea los assemblies provistos para
    /// registrar todas las implementaciones de <see cref="IRequestHandler{TRequest, TResponse}"/>.
    /// Como <see cref="ICommandHandler{TCommand, TResponse}"/> y <see cref="IQueryHandler{TQuery, TResponse}"/>
    /// extienden IRequestHandler, quedan registrados automáticamente.
    /// </summary>
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        services.AddScoped<IMediator, Mediator>();

        if (assemblies.Length == 0)
            return services;

        var openHandler = typeof(IRequestHandler<,>);

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsGenericTypeDefinition)
                .SelectMany(t =>
                    t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == openHandler)
                        .Select(i => new { Service = i, Implementation = t })
                );

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.Service, handler.Implementation);
            }
        }

        return services;
    }
}
