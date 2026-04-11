using DocAutomation.Application.Interfaces;
using DocAutomation.Infrastructure.Persistence;
using DocAutomation.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DocAutomation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IDeploymentRepository, DeploymentRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
