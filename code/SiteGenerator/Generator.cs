using System.Text.Json;
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
        var markdownParser = new MarkdownParser();
        var config = await LoadConfigAsync();
        var backlinks = await BacklinkCollector.CollectBacklinksAsync(
            fileProvider,
            Path.Combine(_contentPath, "thoughts")
        );

        // Process notes (from thoughts folder to notes folder)
        var noteProcessor = new NoteProcessor(
            backlinks,
            _templateRenderer,
            fileProvider,
            markdownParser,
            config
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
            config
        );
        await pageProcessor.ProcessAsync(Path.Combine(_contentPath, "pages"), _outputPath);

        // Copy assets
        fileProvider.CopyFolderAsync(
            Path.Combine(_contentPath, "assets"),
            Path.Combine(_outputPath, "assets")
        );
    }

    private async Task<Config> LoadConfigAsync()
    {
        var configJson = await File.ReadAllTextAsync(_configPath);
        return JsonSerializer.Deserialize<Config>(configJson)
            ?? throw new InvalidOperationException("Failed to load config");
    }
}
