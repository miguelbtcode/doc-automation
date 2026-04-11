using DocAutomation.Domain.Entities;

namespace DocAutomation.Application.Interfaces;

public interface IDeploymentRepository
{
    Task<IReadOnlyList<Deployment>> GetAllAsync(CancellationToken ct = default);
    Task<Deployment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Deployment deployment, CancellationToken ct = default);
    Task<DeploymentStep?> GetStepAsync(
        Guid deploymentId,
        string stepPath,
        CancellationToken ct = default
    );
}
