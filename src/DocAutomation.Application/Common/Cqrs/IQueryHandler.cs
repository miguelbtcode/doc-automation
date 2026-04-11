namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Handler para una query de solo lectura.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> { }
