using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Mediador CQRS. Resuelve handlers via DI, aplica pipeline behaviors en orden inverso
/// y cachea los wrappers tipados en un ConcurrentDictionary para evitar reflection repetido.
/// </summary>
public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerWrapperBase> Wrappers = new();

    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var wrapper =
            (RequestHandlerWrapper<TResponse>)
                Wrappers.GetOrAdd(
                    requestType,
                    static rt =>
                    {
                        var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(
                            rt,
                            GetResponseType(rt)
                        );
                        return (RequestHandlerWrapperBase)Activator.CreateInstance(wrapperType)!;
                    }
                );

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        await Send<Unit>(request, cancellationToken);
    }

    private static Type GetResponseType(Type requestType)
    {
        // Un IRequest<TResponse> implementa la interfaz IRequest<> — extraemos el TResponse.
        var requestInterface = requestType
            .GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>));
        return requestInterface.GetGenericArguments()[0];
    }

    internal abstract class RequestHandlerWrapperBase { }

    internal abstract class RequestHandlerWrapper<TResponse> : RequestHandlerWrapperBase
    {
        public abstract Task<TResponse> Handle(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken
        );
    }

    internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse>
        : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Handle(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken
        )
        {
            var handler = serviceProvider.GetRequiredService<
                IRequestHandler<TRequest, TResponse>
            >();

            Task<TResponse> HandlerDelegate() =>
                handler.Handle((TRequest)request, cancellationToken);

            // Behaviors en orden inverso: el primero registrado es el más externo del pipeline.
            var behaviors = serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .ToArray();

            RequestHandlerDelegate<TResponse> next = HandlerDelegate;
            foreach (var behavior in behaviors)
            {
                var currentBehavior = behavior;
                var currentNext = next;
                next = () =>
                    currentBehavior.Handle((TRequest)request, currentNext, cancellationToken);
            }

            return next();
        }
    }
}
