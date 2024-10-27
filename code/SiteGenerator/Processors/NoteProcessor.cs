using Markdig;
using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class NoteProcessor : IPageProcessor
{
    private readonly Backlinks _backlinks;
    private readonly TemplateRenderer _templateRenderer;

    public NoteProcessor(Backlinks backlinks, TemplateRenderer templateRenderer)
    {
        _backlinks = backlinks;
        _templateRenderer = templateRenderer;
    }

    public async Task ProcessAsync(string inputFile, string outputPath)
    {
        var content = await File.ReadAllTextAsync(inputFile);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlContent = Markdown.ToHtml(content, pipeline);

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        var _noteBacklinks = _backlinks.GetBacklinksForNote(fileName);
        if (_noteBacklinks.Any())
        {
            htmlContent += "<h2>Backlinks</h2><ul>";
            foreach (var link in _noteBacklinks)
            {
                htmlContent += $"<li><a href=\"{link}.html\">{link}</a></li>";
            }
            htmlContent += "</ul>";
        }

        var renderedContent = _templateRenderer.RenderNote(
            new NoteModel(
                htmlContent,
                _noteBacklinks.Select(b => new BacklinkModel(b + ".html", b, "")).ToList()
            ), //TODO: Add preview, fix link
            new LayoutModel("SomeTitle", "SomeDescription", "Website", "SomeUrl", null) //TODO: Add real data
        );

        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        await File.WriteAllTextAsync(outputFile, renderedContent);
    }
}
