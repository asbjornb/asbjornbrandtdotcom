namespace SiteGenerator.Templates;

public interface ITemplateProvider
{
    string GetTemplateContent(string templateName);
    string GetPartialContent(string partialName);
}
