using DocAutomation.Domain.Enums;

namespace DocAutomation.Domain.Entities;

public class Template
{
    public Guid Id { get; set; }
    public TemplateType Type { get; set; } = TemplateType.Deployment;
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string StepsJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    public ICollection<TemplateInput> Inputs { get; set; } = new List<TemplateInput>();
}
