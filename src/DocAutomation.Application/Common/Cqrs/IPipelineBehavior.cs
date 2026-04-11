namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Delegate que representa la siguiente acción en el pipeline (handler o behavior siguiente).
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Comportamiento transversal que envuelve la ejecución de un handler.
/// Permite agregar logging, validación, transacciones, caching, etc. sin tocar los handlers.
/// Se ejecutan en orden de registro antes del handler, y en orden inverso después.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    );
}
