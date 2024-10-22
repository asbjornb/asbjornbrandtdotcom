using HandlebarsDotNet;

namespace SiteGenerator.Templates;

public class TemplateRenderer
{
    private readonly ITemplateProvider templateProvider;

    public TemplateRenderer(ITemplateProvider templateProvider)
    {
        this.templateProvider = templateProvider;
    }

    public async Task<string> RenderAsync(string templateName, object data)
    {
        var templateContent = templateProvider.GetTemplateContent(templateName);

        var template = Handlebars.Compile(templateContent);
        return template(data);
    }
}
