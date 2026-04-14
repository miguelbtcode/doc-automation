using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Deployments.Models;

public class DeploymentListItemDto
{
    public Guid Id { get; set; }
    public TemplateType TemplateType { get; set; }
    public string TemplateSlug { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int TemplateVersion { get; set; }
    public DeploymentStatus Status { get; set; }
    public string? JiraTicketKey { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public int? DurationMinutes =>
        CompletedAt.HasValue ? (int)(CompletedAt.Value - StartedAt).TotalMinutes : null;
}
