namespace DocAutomation.Application.Common.Cqrs;

/// <summary>
/// Representa un valor de retorno vacío para commands sin respuesta.
/// Equivalente a void pero como tipo concreto para poder usarlo en genéricos.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    public static readonly Unit Value = default;
    public static Task<Unit> Task { get; } = System.Threading.Tasks.Task.FromResult(Value);

    public bool Equals(Unit other) => true;

    public override bool Equals(object? obj) => obj is Unit;

    public override int GetHashCode() => 0;

    public override string ToString() => "()";

    public static bool operator ==(Unit left, Unit right) => true;

    public static bool operator !=(Unit left, Unit right) => false;
}
