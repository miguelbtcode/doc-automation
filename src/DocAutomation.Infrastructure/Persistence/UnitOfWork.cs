using DocAutomation.Application.Interfaces;

namespace DocAutomation.Infrastructure.Persistence;

public class UnitOfWork(DocAutomationDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        context.SaveChangesAsync(ct);
}
