namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Handler base para cualquier request que produce una respuesta.
/// Interface interna del sistema CQRS — en código de aplicación usar
/// <see cref="ICommandHandler{TCommand, TResponse}"/> o <see cref="IQueryHandler{TQuery, TResponse}"/>.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Handler para requests sin respuesta (IRequest / Unit).
/// </summary>
public interface IRequestHandler<in TRequest> : IRequestHandler<TRequest, Unit>
    where TRequest : IRequest<Unit> { }
