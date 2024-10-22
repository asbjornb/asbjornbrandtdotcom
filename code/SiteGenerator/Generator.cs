using SiteGenerator.Processors;
using SiteGenerator.Templates;

namespace SiteGenerator;

public class Generator
{
    private readonly string _contentPath;
    private readonly string _outputPath;
    private readonly string _configPath;
    private readonly TemplateRenderer _templateRenderer;
    private readonly BacklinkCollector _backlinkCollector;
    private readonly Dictionary<string, IPageProcessor> _processors;

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
        _backlinkCollector = new BacklinkCollector();

        _processors = new Dictionary<string, IPageProcessor>
        {
            ["thoughts"] = new NoteProcessor(_backlinkCollector, _templateRenderer),
            ["pages"] = new PageProcessor(_templateRenderer),
            ["posts"] = new PostProcessor(_templateRenderer)
        };
    }

    public async Task GenerateSiteAsync()
    {
        await _backlinkCollector.CollectBacklinksAsync(_contentPath);

        foreach (var (subdir, processor) in _processors)
        {
            var inputPath = Path.Combine(_contentPath, subdir);
            var outputPath = Path.Combine(_outputPath, subdir);
            var files = Directory.GetFiles(inputPath, "*.md");

            foreach (var file in files)
            {
                await processor.ProcessAsync(file, outputPath);
            }
        }
    }
}
