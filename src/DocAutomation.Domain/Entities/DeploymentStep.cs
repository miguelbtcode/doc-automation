using DocAutomation.Domain.Enums;

namespace DocAutomation.Domain.Entities;

public class DeploymentStep
{
    public Guid Id { get; set; }
    public Guid DeploymentId { get; set; }
    public string StepPath { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public StepType StepType { get; set; }
    public string Section { get; set; } = string.Empty;
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }

    public Deployment Deployment { get; set; } = null!;
}
