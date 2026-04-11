using System.Reflection;
using DocAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocAutomation.Infrastructure.Persistence;

public class DocAutomationDbContext(DbContextOptions<DocAutomationDbContext> options)
    : DbContext(options)
{
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateInput> TemplateInputs => Set<TemplateInput>();
    public DbSet<Deployment> Deployments => Set<Deployment>();
    public DbSet<DeploymentStep> DeploymentSteps => Set<DeploymentStep>();
    public DbSet<DeploymentHistory> DeploymentHistories => Set<DeploymentHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
