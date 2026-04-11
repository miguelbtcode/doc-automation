using DocAutomation.Application.Features.Templates.Models;

namespace DocAutomation.Application.Interfaces;

public interface ITemplateRenderer
{
    TemplateDocument Render(string stepsJson, IReadOnlyDictionary<string, string> inputValues);
}
