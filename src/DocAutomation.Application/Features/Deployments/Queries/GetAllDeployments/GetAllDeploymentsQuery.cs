using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Deployments.Models;
using DocAutomation.Application.Interfaces;

namespace DocAutomation.Application.Features.Deployments.Queries.GetAllDeployments;

public record GetAllDeploymentsQuery : IQuery<IReadOnlyList<DeploymentListItemDto>>;

public class GetAllDeploymentsQueryHandler(IDeploymentRepository repository)
    : IQueryHandler<GetAllDeploymentsQuery, IReadOnlyList<DeploymentListItemDto>>
{
    public async Task<IReadOnlyList<DeploymentListItemDto>> Handle(
        GetAllDeploymentsQuery request,
        CancellationToken cancellationToken
    )
    {
        var deployments = await repository.GetAllAsync(cancellationToken);
        return deployments
            .Select(d => new DeploymentListItemDto
            {
                Id = d.Id,
                TemplateSlug = d.TemplateSlug,
                TemplateName = d.TemplateName,
                TemplateVersion = d.TemplateVersion,
                Status = d.Status,
                JiraTicketKey = d.JiraTicketKey,
                StartedAt = d.StartedAt,
                CompletedAt = d.CompletedAt,
                CompletedBy = d.CompletedBy,
            })
            .ToList();
    }
}
