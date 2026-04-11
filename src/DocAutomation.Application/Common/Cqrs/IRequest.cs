namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Marker interface base para cualquier request (command o query) que produce una respuesta.
/// El type parameter NO es covariante a propósito — la covarianza rompe la inferencia
/// de tipos genéricos en <see cref="IMediator.Send{TResponse}"/> cuando se pasa un tipo
/// concreto como CreateTemplateCommand.
/// </summary>
public interface IRequest<TResponse> { }

/// <summary>
/// Request sin respuesta (equivale a IRequest&lt;Unit&gt;).
/// </summary>
public interface IRequest : IRequest<Unit> { }
