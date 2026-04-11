using System.Text.Json;
using DocAutomation.Application.Interfaces;

namespace DocAutomation.Application.Services;

public class JsonValidator : IJsonValidator
{
    private static readonly string[] RequiredRootKeys = ["steps"];
    private static readonly HashSet<string> ValidStepTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "action",
        "verification",
        "decision",
    };

    public JsonValidationResult ValidateTemplateSteps(string stepsJson)
    {
        if (string.IsNullOrWhiteSpace(stepsJson))
            return JsonValidationResult.Failure("El JSON de pasos está vacío.");

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(stepsJson);
        }
        catch (JsonException ex)
        {
            return JsonValidationResult.Failure($"JSON inválido: {ex.Message}");
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return JsonValidationResult.Failure("El JSON raíz debe ser un objeto.");

            var errors = new List<string>();

            foreach (var key in RequiredRootKeys)
            {
                if (!root.TryGetProperty(key, out var prop))
                    errors.Add($"Falta la propiedad requerida '{key}'.");
                else if (prop.ValueKind != JsonValueKind.Array)
                    errors.Add($"La propiedad '{key}' debe ser un array.");
            }

            if (errors.Count > 0)
                return JsonValidationResult.Failure(errors.ToArray());

            if (root.TryGetProperty("steps", out var stepsArray))
                ValidateStepArray(stepsArray, "steps", errors);

            if (root.TryGetProperty("post_steps", out var postSteps))
                ValidateStepArray(postSteps, "post_steps", errors);

            if (root.TryGetProperty("reversion", out var reversion))
                ValidateStepArray(reversion, "reversion", errors);

            return errors.Count == 0
                ? JsonValidationResult.Success()
                : JsonValidationResult.Failure(errors.ToArray());
        }
    }

    private static void ValidateStepArray(JsonElement array, string section, List<string> errors)
    {
        if (array.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"'{section}' debe ser un array.");
            return;
        }

        var index = 0;
        foreach (var step in array.EnumerateArray())
        {
            ValidateStep(step, $"{section}[{index}]", errors);
            index++;
        }
    }

    private static void ValidateStep(JsonElement step, string path, List<string> errors)
    {
        if (step.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"{path} debe ser un objeto.");
            return;
        }

        if (
            !step.TryGetProperty("title", out var titleProp)
            || titleProp.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(titleProp.GetString())
        )
            errors.Add($"{path}.title es requerido y debe ser string no vacío.");

        if (
            !step.TryGetProperty("description", out var descProp)
            || descProp.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(descProp.GetString())
        )
            errors.Add(
                $"{path}.description es requerido — cada paso debe tener una descripción detallada."
            );

        if (
            !step.TryGetProperty("type", out var typeProp)
            || typeProp.ValueKind != JsonValueKind.String
        )
        {
            errors.Add($"{path}.type es requerido y debe ser string.");
        }
        else
        {
            var type = typeProp.GetString();
            if (!ValidStepTypes.Contains(type ?? ""))
                errors.Add(
                    $"{path}.type '{type}' no es válido. Valores permitidos: action, verification, decision."
                );
        }

        if (
            step.TryGetProperty("on_fail", out var onFail)
            && onFail.ValueKind == JsonValueKind.Object
        )
        {
            if (onFail.TryGetProperty("steps", out var onFailSteps))
                ValidateStepArray(onFailSteps, $"{path}.on_fail.steps", errors);
        }
    }
}
