using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Templates.Models;
using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Entities;

namespace DocAutomation.Application.Features.Templates.Commands.UpdateTemplate;

public record UpdateTemplateCommand(
    Guid Id,
    string Slug,
    string Name,
    string? Description,
    string StepsJson,
    IReadOnlyList<TemplateInputDto> Inputs
) : ICommand<bool>;

public class UpdateTemplateCommandHandler(ITemplateRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTemplateCommand, bool>
{
    public async Task<bool> Handle(
        UpdateTemplateCommand request,
        CancellationToken cancellationToken
    )
    {
        var template = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
            return false;

        template.Slug = request.Slug.Trim().ToLowerInvariant();
        template.Name = request.Name.Trim();
        template.Description = request.Description?.Trim();
        template.StepsJson = request.StepsJson;
        template.Version += 1;
        template.UpdatedAt = DateTime.UtcNow;

        template.Inputs.Clear();
        foreach (var (input, index) in request.Inputs.Select((i, idx) => (i, idx)))
        {
            template.Inputs.Add(
                new TemplateInput
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    Key = input.Key.Trim(),
                    Label = input.Label.Trim(),
                    InputType = input.InputType,
                    IsRequired = input.IsRequired,
                    DefaultValue = input.DefaultValue,
                    Options = input.Options,
                    DisplayOrder = input.DisplayOrder > 0 ? input.DisplayOrder : index + 1,
                    HelpText = input.HelpText,
                }
            );
        }

        repository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
