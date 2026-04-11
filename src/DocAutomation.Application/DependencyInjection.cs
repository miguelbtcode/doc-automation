using System.Reflection;
using DocAutomation.Application.Common;
using DocAutomation.Application.Common.Cqrs;
using DocAutomation.Application.Interfaces;
using DocAutomation.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DocAutomation.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddCqrs(assembly);
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddSingleton<IJsonValidator, JsonValidator>();
        services.AddSingleton<ITemplateRenderer, TemplateRendererService>();

        return services;
    }
}
