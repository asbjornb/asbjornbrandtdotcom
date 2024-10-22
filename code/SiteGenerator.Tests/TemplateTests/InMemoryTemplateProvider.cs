using SiteGenerator.Templates;

namespace SiteGenerator.Tests.TemplateTests;

public class InMemoryTemplateProvider(
    Dictionary<string, string> templates,
    Dictionary<string, string> partials
) : ITemplateProvider
{
    public string GetTemplateContent(string templateName)
    {
        if (!templates.TryGetValue(templateName, out var content))
            throw new KeyNotFoundException($"Template '{templateName}' not found.");
        return content;
    }

    public string GetPartialContent(string partialName)
    {
        if (!partials.TryGetValue(partialName, out var content))
            throw new KeyNotFoundException($"Partial '{partialName}' not found.");
        return content;
    }
}
