using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Templates.Models;
using DocAutomation.Application.Interfaces;

namespace DocAutomation.Application.Features.Templates.Queries.GetAllTemplates;

public record GetAllTemplatesQuery : IQuery<IReadOnlyList<TemplateListItemDto>>;

public class GetAllTemplatesQueryHandler(ITemplateRepository repository)
    : IQueryHandler<GetAllTemplatesQuery, IReadOnlyList<TemplateListItemDto>>
{
    public async Task<IReadOnlyList<TemplateListItemDto>> Handle(
        GetAllTemplatesQuery request,
        CancellationToken cancellationToken
    )
    {
        var templates = await repository.GetAllAsync(cancellationToken);

        return templates
            .Select(t => new TemplateListItemDto
            {
                Id = t.Id,
                Slug = t.Slug,
                Name = t.Name,
                Description = t.Description,
                Version = t.Version,
                IsActive = t.IsActive,
                UpdatedAt = t.UpdatedAt,
                InputsCount = t.Inputs.Count,
            })
            .ToList();
    }
}
