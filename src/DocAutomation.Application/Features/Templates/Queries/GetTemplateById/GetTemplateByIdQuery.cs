using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Templates.Models;
using DocAutomation.Application.Interfaces;

namespace DocAutomation.Application.Features.Templates.Queries.GetTemplateById;

public record GetTemplateByIdQuery(Guid Id) : IQuery<TemplateDto?>;

public class GetTemplateByIdQueryHandler(ITemplateRepository repository)
    : IQueryHandler<GetTemplateByIdQuery, TemplateDto?>
{
    public async Task<TemplateDto?> Handle(
        GetTemplateByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var template = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
            return null;

        return new TemplateDto
        {
            Id = template.Id,
            Slug = template.Slug,
            Name = template.Name,
            Description = template.Description,
            Version = template.Version,
            IsActive = template.IsActive,
            StepsJson = template.StepsJson,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            Inputs = template
                .Inputs.OrderBy(i => i.DisplayOrder)
                .Select(i => new TemplateInputDto
                {
                    Id = i.Id,
                    Key = i.Key,
                    Label = i.Label,
                    InputType = i.InputType,
                    IsRequired = i.IsRequired,
                    DefaultValue = i.DefaultValue,
                    Options = i.Options,
                    DisplayOrder = i.DisplayOrder,
                    HelpText = i.HelpText,
                })
                .ToList(),
        };
    }
}
