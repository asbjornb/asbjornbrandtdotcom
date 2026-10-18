using Markdig;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class PageProcessor : IPageProcessor
{
    /// <summary>
    /// Replaced on the homepage with the newest posts.
    /// </summary>
    public const string LatestPostsMarker = "<!-- latest-posts -->";
    private const int LatestPostsCount = 3;

    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _fileProvider;
    private readonly MarkdownParser _markdownParser;
    private readonly MarkdownPageWriter _pageWriter;
    private readonly SiteUrlResolver _urlResolver;
    private readonly LayoutModelFactory _layoutFactory;
    private readonly IReadOnlyList<PostSummaryModel> _posts;

    public PageProcessor(
        TemplateRenderer templateRenderer,
        IFileProvider folderReader,
        MarkdownParser markdownParser,
        SiteMetadata config,
        IReadOnlyList<PostSummaryModel>? posts = null
    )
    {
        _posts = posts ?? [];
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
            if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                htmlContent = InsertLatestPosts(htmlContent);
            }

            var pageUrl = _urlResolver.Page(fileName);

            var layout = _layoutFactory.CreatePage(pageUrl, htmlContent);

            var renderedContent = _templateRenderer.RenderPage(layout);

            await _pageWriter.WriteAsync(outputPath, fileName, renderedContent);
        }
    }

    private string InsertLatestPosts(string htmlContent)
    {
        if (!htmlContent.Contains(LatestPostsMarker))
        {
            return htmlContent;
        }

        var latestPosts =
            _posts.Count == 0
                ? ""
                : _templateRenderer.RenderLatestPosts(
                    new PostsIndexModel(_posts.Take(LatestPostsCount).ToList())
                );
        return htmlContent.Replace(LatestPostsMarker, latestPosts);
    }
}
