using System.Text.Json;
using System.Text.RegularExpressions;
using DocAutomation.Application.Features.Templates.Models;
using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Services;

public partial class TemplateRendererService : ITemplateRenderer
{
    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariableRegex();

    public TemplateDocument Render(
        string stepsJson,
        IReadOnlyDictionary<string, string> inputValues
    )
    {
        using var doc = JsonDocument.Parse(stepsJson);
        var root = doc.RootElement;

        var document = new TemplateDocument();

        if (root.TryGetProperty("steps", out var steps))
            document.Steps = ParseSteps(steps, "main", inputValues);

        if (root.TryGetProperty("post_steps", out var postSteps))
            document.PostSteps = ParseSteps(postSteps, "post", inputValues);

        if (root.TryGetProperty("reversion", out var reversion))
            document.Reversion = ParseSteps(reversion, "reversion", inputValues);

        return document;
    }

    private static List<StepDefinition> ParseSteps(
        JsonElement array,
        string sectionPath,
        IReadOnlyDictionary<string, string> inputValues
    )
    {
        var result = new List<StepDefinition>();
        if (array.ValueKind != JsonValueKind.Array)
            return result;

        var index = 1;
        foreach (var item in array.EnumerateArray())
        {
            var step = ParseStep(item, $"{sectionPath}.{index}", inputValues);
            result.Add(step);
            index++;
        }

        return result;
    }

    private static StepDefinition ParseStep(
        JsonElement element,
        string stepPath,
        IReadOnlyDictionary<string, string> inputValues
    )
    {
        var title = element.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
        var description = element.TryGetProperty("description", out var d)
            ? d.GetString() ?? ""
            : "";
        var typeStr = element.TryGetProperty("type", out var tp)
            ? tp.GetString() ?? "action"
            : "action";
        var order =
            element.TryGetProperty("order", out var o) && o.ValueKind == JsonValueKind.Number
                ? o.GetInt32()
                : 0;

        var step = new StepDefinition
        {
            Order = order,
            Title = Interpolate(title, inputValues),
            Description = Interpolate(description, inputValues),
            Type = Enum.TryParse<StepType>(typeStr, ignoreCase: true, out var parsedType)
                ? parsedType
                : StepType.Action,
            StepPath = stepPath,
        };

        if (
            element.TryGetProperty("on_fail", out var onFail)
            && onFail.ValueKind == JsonValueKind.Object
            && onFail.TryGetProperty("steps", out var onFailSteps)
        )
        {
            step.OnFail = new OnFailBlock
            {
                Steps = ParseSteps(onFailSteps, $"{stepPath}.on_fail", inputValues),
            };
        }

        return step;
    }

    private static string Interpolate(string text, IReadOnlyDictionary<string, string> values)
    {
        return VariableRegex()
            .Replace(
                text,
                match =>
                {
                    var key = match.Groups[1].Value;
                    return values.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)
                        ? value
                        : $"[MISSING:{key}]";
                }
            );
    }
}
