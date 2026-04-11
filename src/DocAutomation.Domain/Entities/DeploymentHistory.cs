namespace DocAutomation.Domain.Entities;

public class DeploymentHistory
{
    public Guid Id { get; set; }
    public string TemplateSlug { get; set; } = string.Empty;
    public Guid DeploymentId { get; set; }
    public string Outcome { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public string? LessonsLearned { get; set; }
    public DateTime RecordedAt { get; set; }

    public Deployment Deployment { get; set; } = null!;
}
