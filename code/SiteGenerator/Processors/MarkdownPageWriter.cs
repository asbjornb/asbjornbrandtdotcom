using System.IO;

namespace SiteGenerator.Processors;

public class MarkdownPageWriter
{
    private readonly IFileProvider _fileProvider;

    public MarkdownPageWriter(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    /// <summary>
    /// A page named <c>404</c> is written to <c>404.html</c>, which Cloudflare Pages serves for
    /// unknown URLs.
    /// </summary>
    public const string NotFoundFileName = "404.html";

    public static bool IsNotFoundPage(string slug) => slug == "404";

    public Task WriteAsync(string outputBasePath, string slug, string htmlContent)
    {
        var destination =
            slug.Equals("index", StringComparison.OrdinalIgnoreCase)
                ? Path.Combine(outputBasePath, "index.html")
            : IsNotFoundPage(slug) ? Path.Combine(outputBasePath, NotFoundFileName)
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
