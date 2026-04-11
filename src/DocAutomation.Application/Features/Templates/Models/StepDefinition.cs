using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Templates.Models;

public class StepDefinition
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepType Type { get; set; } = StepType.Action;
    public string StepPath { get; set; } = string.Empty;
    public OnFailBlock? OnFail { get; set; }
}

public class OnFailBlock
{
    public List<StepDefinition> Steps { get; set; } = new();
}

public class TemplateDocument
{
    public List<StepDefinition> Steps { get; set; } = new();
    public List<StepDefinition> PostSteps { get; set; } = new();
    public List<StepDefinition> Reversion { get; set; } = new();
}
