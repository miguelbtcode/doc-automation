using System.Text.RegularExpressions;
using DocAutomation.Application.Interfaces;
using FluentValidation;

namespace DocAutomation.Application.Features.Templates.Commands.UpdateTemplate;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    private static readonly Regex SlugRegex = new("^[a-z0-9-]+$", RegexOptions.Compiled);

    public UpdateTemplateCommandValidator(
        ITemplateRepository repository,
        IJsonValidator jsonValidator
    )
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("El slug es requerido.")
            .MaximumLength(100)
            .Must(s => SlugRegex.IsMatch(s))
            .WithMessage("El slug solo puede contener letras minúsculas, números y guiones.")
            .MustAsync(
                async (cmd, slug, ct) =>
                    !await repository.SlugExistsAsync(slug.ToLowerInvariant(), cmd.Id, ct)
            )
            .WithMessage("Ya existe otro template con ese slug.");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.StepsJson)
            .NotEmpty()
            .WithMessage("El JSON de pasos es requerido.")
            .Must(json => jsonValidator.ValidateTemplateSteps(json).IsValid)
            .WithMessage(
                (_, json) => string.Join(" | ", jsonValidator.ValidateTemplateSteps(json).Errors)
            );

        RuleFor(x => x.Inputs).NotEmpty().WithMessage("Debe definir al menos un input.");

        RuleForEach(x => x.Inputs)
            .ChildRules(input =>
            {
                input.RuleFor(i => i.Key).NotEmpty().MaximumLength(100);
                input.RuleFor(i => i.Label).NotEmpty().MaximumLength(200);
            });

        RuleFor(x => x.Inputs)
            .Must(inputs =>
                inputs.Select(i => i.Key.Trim().ToLowerInvariant()).Distinct().Count()
                == inputs.Count
            )
            .WithMessage("Las keys de los inputs deben ser únicas.");
    }
}
