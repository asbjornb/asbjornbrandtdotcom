using HandlebarsDotNet;

namespace SiteGenerator.Templates;

public class TemplateRenderer
{
    private readonly string _templatePath;

    public TemplateRenderer(string templatePath)
    {
        _templatePath = templatePath;
    }

    public async Task<string> RenderAsync(string templateName, object data)
    {
        var templatePath = Path.Combine(_templatePath, $"{templateName}.html");
        var templateContent = await File.ReadAllTextAsync(templatePath);

        var template = Handlebars.Compile(templateContent);
        return template(data);
    }
}
