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
        var folderReader = new FolderReader();
        var backlinks = await BacklinkCollector.CollectBacklinksAsync(
            folderReader,
            _contentPath
        );

        var processors = new Dictionary<string, IPageProcessor>
        {
            ["thoughts"] = new NoteProcessor(backlinks, _templateRenderer, folderReader),
            ["pages"] = new PageProcessor(_templateRenderer, folderReader),
            ["posts"] = new PostProcessor(_templateRenderer, folderReader)
        };

        foreach (var (subdir, processor) in processors)
        {
            var inputPath = Path.Combine(_contentPath, subdir);
            var outputPath = Path.Combine(_outputPath, subdir);

            await processor.ProcessAsync(inputPath, outputPath);
        }
    }
}
