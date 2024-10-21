using Markdig;

namespace SiteGenerator;

public class NoteProcessor : IPageProcessor
{
    private readonly BacklinkCollector _backlinkCollector;
    private readonly TemplateRenderer _templateRenderer;

    public NoteProcessor(BacklinkCollector backlinkCollector, TemplateRenderer templateRenderer)
    {
        _backlinkCollector = backlinkCollector;
        _templateRenderer = templateRenderer;
    }

    public async Task ProcessAsync(string inputFile, string outputPath)
    {
        var content = await File.ReadAllTextAsync(inputFile);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlContent = Markdown.ToHtml(content, pipeline);

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        var backlinks = _backlinkCollector.GetBacklinksForNote(fileName);
        if (backlinks.Any())
        {
            htmlContent += "<h2>Backlinks</h2><ul>";
            foreach (var link in backlinks)
            {
                htmlContent += $"<li><a href=\"{link}.html\">{link}</a></li>";
            }
            htmlContent += "</ul>";
        }

        var renderedContent = await _templateRenderer.RenderAsync(
            "note",
            new { Content = htmlContent }
        );

        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        await File.WriteAllTextAsync(outputFile, renderedContent);
    }
}
