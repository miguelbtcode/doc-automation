using System.Text.RegularExpressions;
using DocAutomation.Application.Interfaces;
using FluentValidation;

namespace DocAutomation.Application.Features.Templates.Commands.CreateTemplate;

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    private static readonly Regex SlugRegex = new("^[a-z0-9-]+$", RegexOptions.Compiled);

    public CreateTemplateCommandValidator(
        ITemplateRepository repository,
        IJsonValidator jsonValidator
    )
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("El slug es requerido.")
            .MaximumLength(100)
            .Must(s => SlugRegex.IsMatch(s))
            .WithMessage("El slug solo puede contener letras minúsculas, números y guiones.")
            .MustAsync(
                async (slug, ct) =>
                    !await repository.SlugExistsAsync(slug.ToLowerInvariant(), null, ct)
            )
            .WithMessage("Ya existe un template con ese slug.");

        RuleFor(x => x.Name).NotEmpty().WithMessage("El nombre es requerido.").MaximumLength(200);

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

        RuleFor(x => x.CreatedBy).NotEmpty().MaximumLength(100);
    }
}
