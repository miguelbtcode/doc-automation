using System.Text.Json;
using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Features.Templates.Models;
using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Entities;
using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Deployments.Commands.StartDeployment;

public record StartDeploymentCommand(
    Guid TemplateId,
    IReadOnlyDictionary<string, string> InputValues,
    string StartedBy
) : ICommand<Guid>;

public class StartDeploymentCommandHandler(
    ITemplateRepository templateRepository,
    IDeploymentRepository deploymentRepository,
    ITemplateRenderer renderer,
    IUnitOfWork unitOfWork
) : ICommandHandler<StartDeploymentCommand, Guid>
{
    public async Task<Guid> Handle(
        StartDeploymentCommand request,
        CancellationToken cancellationToken
    )
    {
        var template =
            await templateRepository.GetByIdAsync(request.TemplateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {request.TemplateId} no encontrado.");

        var rendered = renderer.Render(template.StepsJson, request.InputValues);

        var deployment = new Deployment
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            TemplateType = template.Type,
            TemplateSlug = template.Slug,
            TemplateName = template.Name,
            TemplateVersion = template.Version,
            InputValuesJson = JsonSerializer.Serialize(request.InputValues),
            RenderedStepsJson = JsonSerializer.Serialize(rendered),
            Status = DeploymentStatus.InProgress,
            StartedAt = DateTime.UtcNow,
            CompletedBy = request.StartedBy,
        };

        var displayOrder = 1;

        foreach (var step in FlattenSteps(rendered.Steps, "main"))
            deployment.Steps.Add(CreateDeploymentStep(step, "main", displayOrder++));

        foreach (var step in FlattenSteps(rendered.PostSteps, "post"))
            deployment.Steps.Add(CreateDeploymentStep(step, "post", displayOrder++));

        foreach (var step in FlattenSteps(rendered.Reversion, "reversion"))
            deployment.Steps.Add(CreateDeploymentStep(step, "reversion", displayOrder++));

        await deploymentRepository.AddAsync(deployment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return deployment.Id;
    }

    private static IEnumerable<StepDefinition> FlattenSteps(
        List<StepDefinition> steps,
        string section
    )
    {
        foreach (var step in steps)
        {
            yield return step;
            if (step.OnFail is not null)
            {
                foreach (var subStep in FlattenSteps(step.OnFail.Steps, section))
                    yield return subStep;
            }
        }
    }

    private static DeploymentStep CreateDeploymentStep(
        StepDefinition def,
        string section,
        int displayOrder
    )
    {
        return new DeploymentStep
        {
            Id = Guid.NewGuid(),
            StepPath = def.StepPath,
            Title = def.Title,
            StepType = def.Type,
            Section = section,
            Status = StepStatus.Pending,
            DisplayOrder = displayOrder,
        };
    }
}
