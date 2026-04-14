using System.Text.Json;
using System.Text.Json.Nodes;
using DocAutomation.Domain.Enums;

namespace DocAutomation.Web.Components.Shared;

public static class StepsJsonConverter
{
    public static StepsEditorModel FromJson(string json)
    {
        var model = new StepsEditorModel();
        if (string.IsNullOrWhiteSpace(json))
            return model;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("steps", out var steps))
                model.MainSteps = ParseStepArray(steps);
            if (root.TryGetProperty("post_steps", out var postSteps))
                model.PostSteps = ParseStepArray(postSteps);
            if (root.TryGetProperty("reversion", out var reversion))
                model.ReversionSteps = ParseStepArray(reversion);
        }
        catch
        {
            // JSON inválido — devolver modelo vacío
        }

        return model;
    }

    public static string WoContentToJson(string htmlContent)
    {
        var root = new JsonObject
        {
            ["steps"] = new JsonArray
            {
                new JsonObject
                {
                    ["order"] = 1,
                    ["title"] = "description",
                    ["type"] = "action",
                    ["description"] = htmlContent,
                },
            },
            ["post_steps"] = new JsonArray(),
            ["reversion"] = new JsonArray(),
        };
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    public static string WoContentFromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("steps", out var steps))
            {
                foreach (var step in steps.EnumerateArray())
                {
                    if (step.TryGetProperty("description", out var d))
                        return d.GetString() ?? string.Empty;
                }
            }
        }
        catch { }
        return string.Empty;
    }

    public static string ToJson(StepsEditorModel model)
    {
        var root = new JsonObject
        {
            ["steps"] = StepArrayToJson(model.MainSteps),
            ["post_steps"] = StepArrayToJson(model.PostSteps),
            ["reversion"] = StepArrayToJson(model.ReversionSteps),
        };

        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static List<StepEditModel> ParseStepArray(JsonElement array)
    {
        var list = new List<StepEditModel>();
        if (array.ValueKind != JsonValueKind.Array)
            return list;

        var order = 0;
        foreach (var item in array.EnumerateArray())
        {
            order++;
            list.Add(ParseStep(item, order));
        }
        return list;
    }

    private static StepEditModel ParseStep(JsonElement element, int defaultOrder)
    {
        var step = new StepEditModel
        {
            Order =
                element.TryGetProperty("order", out var o) && o.ValueKind == JsonValueKind.Number
                    ? o.GetInt32()
                    : defaultOrder,
            Title = element.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
            Description = element.TryGetProperty("description", out var d)
                ? d.GetString() ?? ""
                : "",
            Type =
                element.TryGetProperty("type", out var tp)
                && Enum.TryParse<StepType>(tp.GetString(), ignoreCase: true, out var parsed)
                    ? parsed
                    : StepType.Action,
        };

        if (
            element.TryGetProperty("on_fail", out var onFail)
            && onFail.ValueKind == JsonValueKind.Object
            && onFail.TryGetProperty("steps", out var onFailSteps)
        )
        {
            step.OnFailSteps = ParseStepArray(onFailSteps);
        }

        return step;
    }

    private static JsonArray StepArrayToJson(List<StepEditModel> steps)
    {
        var array = new JsonArray();
        var order = 0;
        foreach (var step in steps)
        {
            order++;
            var obj = new JsonObject
            {
                ["order"] = order,
                ["title"] = step.Title,
                ["type"] = step.Type.ToString().ToLowerInvariant(),
                ["description"] = step.Description,
            };

            if (step.OnFailSteps.Count > 0)
            {
                obj["on_fail"] = new JsonObject { ["steps"] = StepArrayToJson(step.OnFailSteps) };
            }

            array.Add(obj);
        }
        return array;
    }
}
