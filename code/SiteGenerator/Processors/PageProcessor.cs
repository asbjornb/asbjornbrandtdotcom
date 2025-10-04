using Markdig;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class PageProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _fileProvider;
    private readonly MarkdownParser _markdownParser;
    private readonly SiteMetadata _config;
    private readonly MarkdownPageWriter _pageWriter;
    private readonly SiteUrlResolver _urlResolver;

    public PageProcessor(
        TemplateRenderer templateRenderer,
        IFileProvider folderReader,
        MarkdownParser markdownParser,
        SiteMetadata config
    )
    {
        _templateRenderer = templateRenderer;
        _fileProvider = folderReader;
        _markdownParser = markdownParser;
        _config = config;
        _pageWriter = new MarkdownPageWriter(folderReader);
        _urlResolver = new SiteUrlResolver(config);
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        await foreach (var contentFile in _fileProvider.GetFileContents(inputPath, "*.md"))
        {
            var htmlContent = _markdownParser.ParseToHtml(contentFile.Content);

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var pageUrl = _urlResolver.Page(fileName);

            var renderedContent = _templateRenderer.RenderPage(
                new LayoutModel(
                    _config.SiteTitle,
                    _config.Description,
                    "website",
                    pageUrl,
                    htmlContent
                )
            );

            await _pageWriter.WriteAsync(outputPath, fileName, renderedContent);
        }
    }
}
