using System.Text.Json;
using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Configuration;
using SiteGenerator.Feeds;
using SiteGenerator.Processors;
using SiteGenerator.Templates;

namespace SiteGenerator;

public class Generator
{
    private readonly string _contentPath;
    private readonly string _outputPath;
    private readonly SiteMetadata _siteMetadata;
    private readonly TemplateRenderer _templateRenderer;

    /// <summary>
    /// After generation: unpublished notes that published content links to, mapped to the
    /// linking pages. Empty when every note is published.
    /// </summary>
    public IReadOnlyDictionary<string, SortedSet<string>> LinksToUnpublishedNotes
    {
        get;
        private set;
    } = new Dictionary<string, SortedSet<string>>();

    public Generator(
        string contentPath,
        string outputPath,
        TemplateRenderer templateRenderer,
        SiteMetadata siteMetadata
    )
    {
        _contentPath = contentPath;
        _outputPath = outputPath;
        _siteMetadata = siteMetadata;
        _templateRenderer = templateRenderer;
    }

    public async Task GenerateSiteAsync()
    {
        var fileProvider = new FileProvider();
        var publishedNotes = await PublishedNotes.LoadAsync(_contentPath);
        var markdownParser = new MarkdownParser(publishedNotes);
        var backlinks = await BacklinkCollector.CollectBacklinksAsync(
            fileProvider,
            Path.Combine(_contentPath, "thoughts"),
            publishedNotes
        );

        // Generate knowledge graph data first
        var graphProcessor = new GraphProcessor(
            _templateRenderer,
            fileProvider,
            markdownParser,
            _siteMetadata,
            publishedNotes
        );
        var graphData = await graphProcessor.GetGraphDataAsync(
            Path.Combine(_contentPath, "thoughts")
        );

        // Process notes (from thoughts folder to notes folder) with graph data
        var noteProcessor = new NoteProcessor(
            backlinks,
            _templateRenderer,
            fileProvider,
            markdownParser,
            _siteMetadata,
            graphData,
            publishedNotes
        );
        await noteProcessor.ProcessAsync(
            Path.Combine(_contentPath, "thoughts"),
            Path.Combine(_outputPath, "notes")
        );

        // Process pages (from pages folder to root)
        var pageProcessor = new PageProcessor(
            _templateRenderer,
            fileProvider,
            markdownParser,
            _siteMetadata
        );
        await pageProcessor.ProcessAsync(Path.Combine(_contentPath, "pages"), _outputPath);

        var urlResolver = new SiteUrlResolver(_siteMetadata);
        var sitemapUrls = new List<string>(noteProcessor.SitemapUrls);
        sitemapUrls.AddRange(pageProcessor.SitemapUrls);

        // Process posts (from posts folder to posts folder) - only if posts folder exists
        var postsPath = Path.Combine(_contentPath, "posts");
        if (Directory.Exists(postsPath))
        {
            var postProcessor = new PostProcessor(
                _templateRenderer,
                fileProvider,
                markdownParser,
                _siteMetadata
            );
            await postProcessor.ProcessAsync(postsPath, _outputPath);

            await fileProvider.WriteFileAsync(
                Path.Combine(_outputPath, "posts", "feed.xml"),
                RssFeedWriter.Build(
                    _siteMetadata,
                    urlResolver.PostsIndex(),
                    urlResolver.PostsFeed(),
                    postProcessor.Posts
                )
            );

            sitemapUrls.Add(urlResolver.PostsIndex());
            sitemapUrls.AddRange(postProcessor.Posts.Select(p => p.Url));
        }

        await fileProvider.WriteFileAsync(
            Path.Combine(_outputPath, "sitemap.xml"),
            SitemapWriter.Build(sitemapUrls)
        );

        // Generate knowledge graph JSON data for frontend
        await graphProcessor.ProcessAsync(Path.Combine(_contentPath, "thoughts"), _outputPath);

        // Copy assets (if assets folder exists)
        var assetsPath = Path.Combine(_contentPath, "assets");
        if (Directory.Exists(assetsPath))
        {
            fileProvider.CopyFolderAsync(assetsPath, Path.Combine(_outputPath, "assets"));
        }

        // Copy root-level static files such as _redirects (if static folder exists)
        var staticPath = Path.Combine(_contentPath, "static");
        if (Directory.Exists(staticPath))
        {
            fileProvider.CopyFolderAsync(staticPath, _outputPath);
        }

        if (!publishedNotes.PublishesEverything)
        {
            LinksToUnpublishedNotes = await FindLinksToUnpublishedNotesAsync(
                fileProvider,
                publishedNotes
            );
            GenerationWarnings.LinksToUnpublishedNotes(LinksToUnpublishedNotes);
        }
    }

    private async Task<
        IReadOnlyDictionary<string, SortedSet<string>>
    > FindLinksToUnpublishedNotesAsync(IFileProvider fileProvider, PublishedNotes publishedNotes)
    {
        // Unpublished note -> published pages linking to it
        var linkedFrom = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);

        foreach (
            var (folder, urlPrefix) in new[]
            {
                ("thoughts", "notes/"),
                ("pages", ""),
                ("posts", "posts/"),
            }
        )
        {
            var folderPath = Path.Combine(_contentPath, folder);
            if (!Directory.Exists(folderPath))
            {
                continue;
            }

            await foreach (var file in fileProvider.GetFileContents(folderPath, "*.md"))
            {
                if (folder == "thoughts" && !publishedNotes.IsPublishedFile(file.Name))
                {
                    continue;
                }

                var source = urlPrefix + Path.GetFileNameWithoutExtension(file.Name);
                foreach (var target in WikiLinks.Targets(file.Content))
                {
                    if (publishedNotes.IsPublished(target))
                    {
                        continue;
                    }

                    if (!linkedFrom.TryGetValue(target, out var sources))
                    {
                        sources = new SortedSet<string>(StringComparer.Ordinal);
                        linkedFrom[target] = sources;
                    }
                    sources.Add(source);
                }
            }
        }

        return linkedFrom;
    }
}
