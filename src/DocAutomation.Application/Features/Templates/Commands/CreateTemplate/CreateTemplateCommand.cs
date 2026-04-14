using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Templates.Models;
using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Entities;

namespace DocAutomation.Application.Features.Templates.Commands.CreateTemplate;

public record CreateTemplateCommand(
    string Slug,
    string Name,
    string? Description,
    string StepsJson,
    IReadOnlyList<TemplateInputDto> Inputs,
    string CreatedBy,
    TemplateType Type = TemplateType.Deployment
) : ICommand<Guid>;

public class CreateTemplateCommandHandler(ITemplateRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTemplateCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateTemplateCommand request,
        CancellationToken cancellationToken
    )
    {
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Slug = request.Slug.Trim().ToLowerInvariant(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            StepsJson = request.StepsJson,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            Inputs = request
                .Inputs.Select(
                    (i, index) =>
                        new TemplateInput
                        {
                            Id = Guid.NewGuid(),
                            Key = i.Key.Trim(),
                            Label = i.Label.Trim(),
                            InputType = i.InputType,
                            IsRequired = i.IsRequired,
                            DefaultValue = i.DefaultValue,
                            Options = i.Options,
                            DisplayOrder = i.DisplayOrder > 0 ? i.DisplayOrder : index + 1,
                            HelpText = i.HelpText,
                        }
                )
                .ToList(),
        };

        await repository.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
