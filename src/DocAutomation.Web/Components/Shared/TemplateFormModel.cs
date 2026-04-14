using DocAutomation.Application.Features.Deployments.Models;
using DocAutomation.Application.Features.Templates.Models;

namespace DocAutomation.Web.Components.Shared;

public class TemplateFormModel
{
    public Guid? Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<TemplateInputDto> Inputs { get; set; } = new();
    public StepsEditorModel Steps { get; set; } = new();
    public string WoContent { get; set; } = string.Empty;
}
