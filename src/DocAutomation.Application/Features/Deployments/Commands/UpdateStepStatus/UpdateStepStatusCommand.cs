using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Deployments.Commands.UpdateStepStatus;

public record UpdateStepStatusCommand(
    Guid DeploymentId,
    string StepPath,
    StepStatus Status,
    string? Notes
) : ICommand<bool>;

public class UpdateStepStatusCommandHandler(
    IDeploymentRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateStepStatusCommand, bool>
{
    public async Task<bool> Handle(
        UpdateStepStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        var step = await repository.GetStepAsync(
            request.DeploymentId,
            request.StepPath,
            cancellationToken
        );
        if (step is null)
            return false;

        step.Status = request.Status;
        step.Notes = request.Notes;
        step.CompletedAt = request.Status
            is StepStatus.Completed
                or StepStatus.Failed
                or StepStatus.Skipped
            ? DateTime.UtcNow
            : null;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
