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

    public async Task<bool> ReplaceAsync(
        Guid id,
        string slug,
        string name,
        string? description,
        string stepsJson,
        IEnumerable<TemplateInput> newInputs,
        CancellationToken ct = default
    )
    {
        var exists = await context.Templates.AnyAsync(t => t.Id == id, ct);
        if (!exists)
            return false;

        await context.TemplateInputs.Where(i => i.TemplateId == id).ExecuteDeleteAsync(ct);

        var now = DateTime.UtcNow;
        var trimmedDescription = description?.Trim();
        await context
            .Templates.Where(t => t.Id == id)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(t => t.Slug, slug.Trim().ToLowerInvariant())
                        .SetProperty(t => t.Name, name.Trim())
                        .SetProperty(t => t.Description, trimmedDescription)
                        .SetProperty(t => t.StepsJson, stepsJson)
                        .SetProperty(t => t.UpdatedAt, now)
                        .SetProperty(t => t.Version, t => t.Version + 1),
                ct
            );

        foreach (var input in newInputs)
        {
            input.TemplateId = id;
            await context.TemplateInputs.AddAsync(input, ct);
        }

        return true;
    }
}
