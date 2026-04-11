namespace DocAutomation.Application.Features.Templates.Models;

public class TemplateListItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int InputsCount { get; set; }
}
