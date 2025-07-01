using System.Globalization;
using System.Text.RegularExpressions;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class PostProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _fileProvider;
    private readonly MarkdownParser _markdownParser;
    private readonly SiteMetadata _siteMetadata;

    public PostProcessor(
        TemplateRenderer templateRenderer,
        IFileProvider fileProvider,
        MarkdownParser markdownParser,
        SiteMetadata siteMetadata
    )
    {
        _templateRenderer = templateRenderer;
        _fileProvider = fileProvider;
        _markdownParser = markdownParser;
        _siteMetadata = siteMetadata;
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        var posts = new List<PostSummaryModel>();

        await foreach (var contentFile in _fileProvider.GetFileContents(inputPath, "*.md"))
        {
            var (date, slug) = ExtractDateAndSlugFromFileName(contentFile.Name);
            var htmlContent = _markdownParser.ParseToHtml(contentFile.Content);
            var title = ExtractTitle(htmlContent) ?? slug;

            // Remove the H1 from content since we're displaying it in the post header
            var contentWithoutH1 = RemoveFirstH1(htmlContent);

            var postUrl = $"{_siteMetadata.BaseUrl}/posts/{slug}/";

            // Create individual post
            var postModel = new PostModel(title, contentWithoutH1, date, slug, postUrl);
            var layoutModel = new LayoutModel(
                $"{title} • {_siteMetadata.SiteTitle}",
                _siteMetadata.Description,
                "article",
                postUrl,
                null // Will be set by template renderer
            );

            var renderedPost = _templateRenderer.RenderPost(postModel, layoutModel);

            var postFolder = Path.Combine(outputPath, "posts", slug);
            await _fileProvider.WriteFileAsync(
                Path.Combine(postFolder, "index.html"),
                renderedPost
            );

            // Add to posts list for index
            posts.Add(new PostSummaryModel(title, postUrl, date, slug));
        }

        // Create posts index page
        await CreatePostsIndexPage(posts, outputPath);
    }

    private async Task CreatePostsIndexPage(List<PostSummaryModel> posts, string outputPath)
    {
        // Sort posts by date (newest first)
        var sortedPosts = posts.OrderByDescending(p => p.PublishedDate).ToList();

        var postsIndexModel = new PostsIndexModel(sortedPosts);
        var layoutModel = new LayoutModel(
            $"Posts • {_siteMetadata.SiteTitle}",
            _siteMetadata.Description,
            "website",
            $"{_siteMetadata.BaseUrl}/posts/",
            null // Will be set by template renderer
        );

        var renderedIndex = _templateRenderer.RenderPostsIndex(postsIndexModel, layoutModel);

        var postsFolder = Path.Combine(outputPath, "posts");
        await _fileProvider.WriteFileAsync(Path.Combine(postsFolder, "index.html"), renderedIndex);
    }

    private static (DateTime date, string slug) ExtractDateAndSlugFromFileName(string fileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        // Pattern: YYYY-MM-DD-slug-here
        var match = Regex.Match(fileNameWithoutExtension, @"^(\d{4}-\d{2}-\d{2})-(.+)$");

        if (!match.Success)
        {
            throw new ArgumentException(
                $"Post filename '{fileName}' must follow the pattern 'YYYY-MM-DD-slug.md'"
            );
        }

        var dateString = match.Groups[1].Value;
        var slug = match.Groups[2].Value;

        if (
            !DateTime.TryParseExact(
                dateString,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date
            )
        )
        {
            throw new ArgumentException(
                $"Invalid date format in filename '{fileName}'. Expected YYYY-MM-DD."
            );
        }

        return (date, slug);
    }

    private static string? ExtractTitle(string htmlContent)
    {
        var match = Regex.Match(htmlContent, @"<h1[^>]*>(.*?)</h1>", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string RemoveFirstH1(string htmlContent)
    {
        return Regex
            .Replace(
                htmlContent,
                @"<h1[^>]*>.*?</h1>\s*",
                "",
                RegexOptions.IgnoreCase | RegexOptions.Singleline,
                TimeSpan.FromSeconds(1)
            )
            .Trim();
    }
}
