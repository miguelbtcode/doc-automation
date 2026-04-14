using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocAutomation.Infrastructure.Persistence.Repositories;

public class DeploymentRepository(DocAutomationDbContext context) : IDeploymentRepository
{
    public async Task<IReadOnlyList<Deployment>> GetAllAsync(
        TemplateType? type = null,
        CancellationToken ct = default
    )
    {
        var query = context.Deployments.AsQueryable();
        if (type.HasValue)
            query = query.Where(d => d.TemplateType == type.Value);
        return await query.OrderByDescending(d => d.StartedAt).ToListAsync(ct);
    }

    public Task<Deployment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return context
            .Deployments.Include(d => d.Steps.OrderBy(s => s.DisplayOrder))
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task AddAsync(Deployment deployment, CancellationToken ct = default)
    {
        await context.Deployments.AddAsync(deployment, ct);
    }

    public Task<DeploymentStep?> GetStepAsync(
        Guid deploymentId,
        string stepPath,
        CancellationToken ct = default
    )
    {
        return context.DeploymentSteps.FirstOrDefaultAsync(
            s => s.DeploymentId == deploymentId && s.StepPath == stepPath,
            ct
        );
    }
}
