using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Interfaces;

namespace DocAutomation.Application.Features.Templates.Commands.DeleteTemplate;

public record DeleteTemplateCommand(Guid Id) : ICommand<bool>;

public class DeleteTemplateCommandHandler(ITemplateRepository repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTemplateCommand, bool>
{
    public async Task<bool> Handle(
        DeleteTemplateCommand request,
        CancellationToken cancellationToken
    )
    {
        var template = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
            return false;

        repository.Remove(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
