using DocAutomation.Domain.Enums;

namespace DocAutomation.Domain.Entities;

public class Deployment
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateSlug { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public int TemplateVersion { get; set; }
    public string InputValuesJson { get; set; } = string.Empty;
    public string RenderedStepsJson { get; set; } = string.Empty;
    public DeploymentStatus Status { get; set; } = DeploymentStatus.InProgress;
    public string? JiraTicketKey { get; set; }
    public string? JiraTicketUrl { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CompletedBy { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public Template Template { get; set; } = null!;
    public ICollection<DeploymentStep> Steps { get; set; } = new List<DeploymentStep>();
}
