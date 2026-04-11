using FluentValidation;

namespace DocAutomation.Application.Features.Deployments.Commands.StartDeployment;

public class StartDeploymentCommandValidator : AbstractValidator<StartDeploymentCommand>
{
    public StartDeploymentCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Debe seleccionar un template.");
        RuleFor(x => x.StartedBy).NotEmpty().MaximumLength(100);
        RuleFor(x => x.InputValues).NotNull();
    }
}
