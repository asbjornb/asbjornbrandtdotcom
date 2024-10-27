using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Processors;
using SiteGenerator.Templates;

namespace SiteGenerator;

public class Generator
{
    private readonly string _contentPath;
    private readonly string _outputPath;
    private readonly string _configPath;
    private readonly TemplateRenderer _templateRenderer;

    public Generator(
        string contentPath,
        string outputPath,
        TemplateRenderer templateRenderer,
        string configPath
    )
    {
        _contentPath = contentPath;
        _outputPath = outputPath;
        _configPath = configPath;
        _templateRenderer = templateRenderer;
    }

    public async Task GenerateSiteAsync()
    {
        var fileProvider = new FileProvider();
        var markdownConverter = new MarkdownConverter();
        var backlinks = await BacklinkCollector.CollectBacklinksAsync(fileProvider, _contentPath);

        // Process notes (from thoughts folder to notes folder)
        var noteProcessor = new NoteProcessor(
            backlinks,
            _templateRenderer,
            fileProvider,
            markdownConverter
        );
        await noteProcessor.ProcessAsync(
            Path.Combine(_contentPath, "thoughts"),
            Path.Combine(_outputPath, "notes")
        );

        // Process pages
        var pageProcessor = new PageProcessor(_templateRenderer, fileProvider);
        await pageProcessor.ProcessAsync(
            Path.Combine(_contentPath, "pages"),
            Path.Combine(_outputPath, "pages")
        );
    }
}
