using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Deployments.Models;

public class DeploymentStepDto
{
    public Guid Id { get; set; }
    public string StepPath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public string Section { get; set; } = string.Empty;
    public StepStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
}
