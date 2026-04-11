using DocAutomation.Domain.Enums;

namespace DocAutomation.Web.Components.Shared;

public class StepEditModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public StepType Type { get; set; } = StepType.Action;
    public List<StepEditModel> OnFailSteps { get; set; } = new();
}

public class StepsEditorModel
{
    public List<StepEditModel> MainSteps { get; set; } = new();
    public List<StepEditModel> PostSteps { get; set; } = new();
    public List<StepEditModel> ReversionSteps { get; set; } = new();
}
