namespace DocAutomation.Application.Interfaces;

public record JsonValidationResult(bool IsValid, IReadOnlyList<string> Errors)
{
    public static JsonValidationResult Success() => new(true, Array.Empty<string>());

    public static JsonValidationResult Failure(params string[] errors) => new(false, errors);
}

public interface IJsonValidator
{
    JsonValidationResult ValidateTemplateSteps(string stepsJson);
}
