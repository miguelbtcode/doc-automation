namespace DocAutomation.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool isSuccess, IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, Array.Empty<string>());

    public static Result Failure(params string[] errors) => new(false, errors);

    public static Result Failure(IEnumerable<string> errors) => new(false, errors.ToList());

    public static Result<T> Success<T>(T value) => Result<T>.SuccessResult(value);

    public static Result<T> Failure<T>(params string[] errors) => Result<T>.FailureResult(errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T? value, bool isSuccess, IReadOnlyList<string> errors)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    internal static Result<T> SuccessResult(T value) => new(value, true, Array.Empty<string>());

    internal static Result<T> FailureResult(params string[] errors) => new(default, false, errors);
}
