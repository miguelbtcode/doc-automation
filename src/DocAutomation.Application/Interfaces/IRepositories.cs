using DocAutomation.Domain.Entities;

namespace DocAutomation.Application.Interfaces;

public interface ITemplateRepository
{
    Task<IReadOnlyList<Template>> GetAllAsync(CancellationToken ct = default);
    Task<Template?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Template?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken ct = default);
    Task AddAsync(Template template, CancellationToken ct = default);
    void Update(Template template);
    void Remove(Template template);

    /// <summary>Replaces full template (data + inputs) using direct SQL, bypassing change tracker.</summary>
    Task<bool> ReplaceAsync(
        Guid id,
        string slug,
        string name,
        string? description,
        string stepsJson,
        IEnumerable<TemplateInput> newInputs,
        CancellationToken ct = default
    );
}

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

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
