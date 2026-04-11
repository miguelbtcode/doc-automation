namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Representa una operación que muta estado y devuelve una respuesta.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse> { }

/// <summary>
/// Representa una operación que muta estado sin devolver valor.
/// </summary>
public interface ICommand : ICommand<Unit> { }
