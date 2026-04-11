namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Representa una operación de solo lectura que devuelve datos sin mutar estado.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse> { }
