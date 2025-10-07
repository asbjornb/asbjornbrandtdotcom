using System.IO;

namespace SiteGenerator.Processors;

public class MarkdownPageWriter
{
    private readonly IFileProvider _fileProvider;

    public MarkdownPageWriter(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public Task WriteAsync(string outputBasePath, string slug, string htmlContent)
    {
        var destination = slug.Equals("index", StringComparison.OrdinalIgnoreCase)
            ? Path.Combine(outputBasePath, "index.html")
            : Path.Combine(outputBasePath, slug, "index.html");

        return _fileProvider.WriteFileAsync(destination, htmlContent);
    }

    public Task WriteInSectionAsync(
        string outputBasePath,
        string sectionName,
        string slug,
        string htmlContent
    )
    {
        var sectionPath = Path.Combine(outputBasePath, sectionName);
        Directory.CreateDirectory(sectionPath);
        return WriteAsync(sectionPath, slug, htmlContent);
    }
}
