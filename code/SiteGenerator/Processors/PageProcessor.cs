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
    private readonly MarkdownPageWriter _pageWriter;
    private readonly SiteUrlResolver _urlResolver;
    private readonly LayoutModelFactory _layoutFactory;

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
        _pageWriter = new MarkdownPageWriter(folderReader);
        _urlResolver = new SiteUrlResolver(config);
        _layoutFactory = new LayoutModelFactory(config);
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        await foreach (var contentFile in _fileProvider.GetFileContents(inputPath, "*.md"))
        {
            var htmlContent = _markdownParser.ParseToHtml(contentFile.Content);

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var pageUrl = _urlResolver.Page(fileName);

            var layout = _layoutFactory.CreatePage(pageUrl, htmlContent);

            var renderedContent = _templateRenderer.RenderPage(layout);

            await _pageWriter.WriteAsync(outputPath, fileName, renderedContent);
        }
    }
}
