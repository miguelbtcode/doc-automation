using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Deployments.Models;
using DocAutomation.Application.Interfaces;

namespace DocAutomation.Application.Features.Deployments.Queries.GetDeploymentById;

public record GetDeploymentByIdQuery(Guid Id) : IQuery<DeploymentDto?>;

public class GetDeploymentByIdQueryHandler(IDeploymentRepository repository)
    : IQueryHandler<GetDeploymentByIdQuery, DeploymentDto?>
{
    public async Task<DeploymentDto?> Handle(
        GetDeploymentByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var deployment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (deployment is null)
            return null;

        return new DeploymentDto
        {
            Id = deployment.Id,
            TemplateId = deployment.TemplateId,
            TemplateSlug = deployment.TemplateSlug,
            TemplateName = deployment.TemplateName,
            TemplateVersion = deployment.TemplateVersion,
            InputValuesJson = deployment.InputValuesJson,
            RenderedStepsJson = deployment.RenderedStepsJson,
            Status = deployment.Status,
            JiraTicketKey = deployment.JiraTicketKey,
            JiraTicketUrl = deployment.JiraTicketUrl,
            StartedAt = deployment.StartedAt,
            CompletedAt = deployment.CompletedAt,
            CompletedBy = deployment.CompletedBy,
            Notes = deployment.Notes,
            Steps = deployment
                .Steps.OrderBy(s => s.DisplayOrder)
                .Select(s => new DeploymentStepDto
                {
                    Id = s.Id,
                    StepPath = s.StepPath,
                    Title = s.Title,
                    StepType = s.StepType,
                    Section = s.Section,
                    Status = s.Status,
                    CompletedAt = s.CompletedAt,
                    Notes = s.Notes,
                    DisplayOrder = s.DisplayOrder,
                })
                .ToList(),
        };
    }
}
