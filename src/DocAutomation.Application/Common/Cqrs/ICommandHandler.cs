namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Handler para un command que devuelve una respuesta.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> { }

/// <summary>
/// Handler para un command sin respuesta.
/// </summary>
public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
    where TCommand : ICommand { }
