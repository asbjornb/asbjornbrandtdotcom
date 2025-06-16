using System.Text.Json;
using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Configuration;
using SiteGenerator.Processors;
using SiteGenerator.Templates;

namespace SiteGenerator;

public class Generator
{
    private readonly string _contentPath;
    private readonly string _outputPath;
    private readonly SiteMetadata _siteMetadata;
    private readonly TemplateRenderer _templateRenderer;

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
        var markdownParser = new MarkdownParser();
        var backlinks = await BacklinkCollector.CollectBacklinksAsync(
            fileProvider,
            Path.Combine(_contentPath, "thoughts")
        );

        // Generate knowledge graph data first
        var graphProcessor = new GraphProcessor(
            _templateRenderer,
            fileProvider,
            markdownParser,
            _siteMetadata
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
            graphData
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
        }

        // Generate knowledge graph JSON data for frontend
        await graphProcessor.ProcessAsync(Path.Combine(_contentPath, "thoughts"), _outputPath);

        // Copy assets
        fileProvider.CopyFolderAsync(
            Path.Combine(_contentPath, "assets"),
            Path.Combine(_outputPath, "assets")
        );
    }
}
