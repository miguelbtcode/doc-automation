using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Interfaces;
using DocAutomation.Domain.Enums;

namespace DocAutomation.Application.Features.Deployments.Commands.CompleteDeployment;

public record CompleteDeploymentCommand(
    Guid DeploymentId,
    DeploymentStatus FinalStatus,
    string? Notes
) : ICommand<bool>;

public class CompleteDeploymentCommandHandler(
    IDeploymentRepository repository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CompleteDeploymentCommand, bool>
{
    public async Task<bool> Handle(
        CompleteDeploymentCommand request,
        CancellationToken cancellationToken
    )
    {
        var deployment = await repository.GetByIdAsync(request.DeploymentId, cancellationToken);
        if (deployment is null)
            return false;

        deployment.Status = request.FinalStatus;
        deployment.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            deployment.Notes = request.Notes;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
