using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Templates.Models;

public class TemplateInputDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public InputType InputType { get; set; } = InputType.Text;
    public bool IsRequired { get; set; } = true;
    public string? DefaultValue { get; set; }
    public string? Options { get; set; }
    public int DisplayOrder { get; set; }
    public string? HelpText { get; set; }
}
