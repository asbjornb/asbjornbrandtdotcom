namespace SiteGenerator.Templates;

public class FileTemplateProvider : ITemplateProvider
{
    private readonly string _templatePath;

    public FileTemplateProvider(string templatePath)
    {
        _templatePath = templatePath;
    }

    public string GetTemplateContent(string templateName)
    {
        var templatePath = Path.Combine(_templatePath, $"{templateName}.html");
        return File.ReadAllText(templatePath);
    }

    public string GetPartialContent(string partialName)
    {
        var partialPath = Path.Combine(_templatePath, "partials", $"{partialName}.html");
        return File.ReadAllText(partialPath);
    }
}
