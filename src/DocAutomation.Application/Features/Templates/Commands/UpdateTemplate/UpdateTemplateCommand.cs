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
        var newInputs = request
            .Inputs.Select(
                (input, index) =>
                    new TemplateInput
                    {
                        Id = Guid.NewGuid(),
                        TemplateId = request.Id,
                        Key = input.Key.Trim(),
                        Label = input.Label.Trim(),
                        InputType = input.InputType,
                        IsRequired = input.IsRequired,
                        DefaultValue = input.DefaultValue,
                        Options = input.Options,
                        DisplayOrder = input.DisplayOrder > 0 ? input.DisplayOrder : index + 1,
                        HelpText = input.HelpText,
                    }
            )
            .ToList();

        var updated = await repository.ReplaceAsync(
            request.Id,
            request.Slug,
            request.Name,
            request.Description,
            request.StepsJson,
            newInputs,
            cancellationToken
        );

        if (!updated)
            return false;

        // Solo los inputs nuevos quedan pendientes en el change tracker → SaveChanges los inserta
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
