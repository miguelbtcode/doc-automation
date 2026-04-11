using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocAutomation.Infrastructure.Persistence.Repositories;

public class TemplateRepository(DocAutomationDbContext context) : ITemplateRepository
{
    public async Task<IReadOnlyList<Template>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Templates.Include(t => t.Inputs).OrderBy(t => t.Name).ToListAsync(ct);
    }

    public Task<Template?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return context
            .Templates.Include(t => t.Inputs.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public Task<Template?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return context
            .Templates.Include(t => t.Inputs.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken ct = default
    )
    {
        var query = context.Templates.IgnoreQueryFilters().Where(t => t.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(t => t.Id != excludeId.Value);
        return query.AnyAsync(ct);
    }

    public async Task AddAsync(Template template, CancellationToken ct = default)
    {
        await context.Templates.AddAsync(template, ct);
    }

    public void Update(Template template) => context.Templates.Update(template);

    public void Remove(Template template)
    {
        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;
        context.Templates.Update(template);
    }
}
