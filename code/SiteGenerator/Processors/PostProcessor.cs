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
    private readonly MarkdownPageWriter _pageWriter;
    private readonly SiteUrlResolver _urlResolver;
    private readonly LayoutModelFactory _layoutFactory;

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
        _pageWriter = new MarkdownPageWriter(fileProvider);
        _urlResolver = new SiteUrlResolver(siteMetadata);
        _layoutFactory = new LayoutModelFactory(siteMetadata);
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

            // Extract excerpt from content
            var excerpt = ExtractExcerpt(contentWithoutH1);

            var postUrl = _urlResolver.Post(slug);

            // Create individual post
            var postModel = new PostModel(title, contentWithoutH1, date, slug, postUrl);
            var layoutModel = _layoutFactory.CreatePost(title, postUrl);

            var renderedPost = _templateRenderer.RenderPost(postModel, layoutModel);

            await _pageWriter.WriteInSectionAsync(outputPath, "posts", slug, renderedPost);

            // Add to posts list for index
            posts.Add(new PostSummaryModel(title, postUrl, date, slug, excerpt));
        }

        // Create posts index page
        await CreatePostsIndexPage(posts, outputPath);
    }

    private async Task CreatePostsIndexPage(List<PostSummaryModel> posts, string outputPath)
    {
        // Sort posts by date (newest first)
        var sortedPosts = posts.OrderByDescending(p => p.PublishedDate).ToList();

        var postsIndexModel = new PostsIndexModel(sortedPosts);
        var layoutModel = _layoutFactory.CreatePostsIndex(_urlResolver.PostsIndex());

        var renderedIndex = _templateRenderer.RenderPostsIndex(postsIndexModel, layoutModel);

        await _pageWriter.WriteInSectionAsync(outputPath, "posts", "index", renderedIndex);
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

    private static string ExtractExcerpt(string htmlContent)
    {
        const int maxLength = 150;

        // Remove HTML tags
        var plainText = Regex.Replace(htmlContent, @"<[^>]*>", " ");

        // Clean up whitespace
        plainText = Regex.Replace(plainText, @"\s+", " ").Trim();

        // Extract first 2-3 sentences (approximately 150 characters)
        var sentences = plainText.Split('.', '!', '?');
        var excerpt = "";

        foreach (var sentence in sentences)
        {
            var trimmed = sentence.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (excerpt.Length + trimmed.Length + 1 > maxLength)
            {
                // If excerpt is still empty, truncate the first sentence at a word boundary
                if (excerpt.Length == 0)
                {
                    var truncated =
                        trimmed.Length > maxLength ? trimmed[..maxLength].TrimEnd() : trimmed;

                    // Try to break at the last space to avoid cutting words
                    var lastSpace = truncated.LastIndexOf(' ');
                    if (lastSpace > maxLength / 2)
                    {
                        truncated = truncated[..lastSpace];
                    }

                    excerpt = truncated;
                }
                break;
            }

            excerpt += (excerpt.Length > 0 ? ". " : "") + trimmed;
        }

        // Add ellipsis if we truncated
        if (excerpt.Length < plainText.Length && !excerpt.EndsWith('.'))
        {
            excerpt += "...";
        }

        return excerpt;
    }
}
