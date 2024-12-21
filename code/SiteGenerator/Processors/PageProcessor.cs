using Markdig;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class PageProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _folderReader;
    private readonly MarkdownParser _markdownParser;
    private readonly SiteMetadata _config;

    public PageProcessor(
        TemplateRenderer templateRenderer,
        IFileProvider folderReader,
        MarkdownParser markdownParser,
        SiteMetadata config
    )
    {
        _templateRenderer = templateRenderer;
        _folderReader = folderReader;
        _markdownParser = markdownParser;
        _config = config;
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        await foreach (var contentFile in _folderReader.GetFileContents(inputPath, "*.md"))
        {
            var htmlContent = _markdownParser.ParseToHtml(contentFile.Content);

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var pageUrl = fileName.Equals("index", StringComparison.OrdinalIgnoreCase)
                ? _config.BaseUrl
                : $"{_config.BaseUrl}/{fileName}/";

            var renderedContent = _templateRenderer.RenderPage(
                new LayoutModel(
                    _config.SiteTitle,
                    _config.Description,
                    "website",
                    pageUrl,
                    htmlContent
                )
            );

            if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                await _folderReader.WriteFileAsync(
                    Path.Combine(outputPath, "index.html"),
                    renderedContent
                );
            }
            else
            {
                var pageFolder = Path.Combine(outputPath, fileName);
                await _folderReader.WriteFileAsync(
                    Path.Combine(pageFolder, "index.html"),
                    renderedContent
                );
            }
        }
    }
}
