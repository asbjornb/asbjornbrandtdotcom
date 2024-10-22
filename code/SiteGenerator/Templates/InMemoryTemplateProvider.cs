namespace SiteGenerator.Templates;

public class InMemoryTemplateProvider : ITemplateProvider
{
    private readonly Dictionary<string, string> _templates;
    private readonly Dictionary<string, string> _partials;

    public InMemoryTemplateProvider(
        Dictionary<string, string> templates,
        Dictionary<string, string> partials
    )
    {
        _templates = templates;
        _partials = partials;
    }

    public string GetTemplateContent(string templateName)
    {
        return _templates.TryGetValue(templateName, out var content) ? content : null;
    }

    public string GetPartialContent(string partialName)
    {
        return _partials.TryGetValue(partialName, out var content) ? content : null;
    }
}
