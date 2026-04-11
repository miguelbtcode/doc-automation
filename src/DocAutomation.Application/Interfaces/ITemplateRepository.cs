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
}
