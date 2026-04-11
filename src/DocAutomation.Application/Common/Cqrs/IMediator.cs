namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Envía requests (commands/queries) al handler correspondiente ejecutando los pipeline behaviors.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Envía un request y devuelve la respuesta del handler.
    /// </summary>
    Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Envía un command sin respuesta.
    /// </summary>
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}
