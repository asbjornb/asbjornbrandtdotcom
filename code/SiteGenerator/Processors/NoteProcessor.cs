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

    public async Task ProcessAsync(IFolderReader folderReader, string inputPath, string outputPath)
    {
        await foreach (var contentFile in folderReader.GetFileContents(inputPath, "*.md"))
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var htmlContent = Markdown.ToHtml(contentFile.Content, pipeline);

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var noteBacklinks = _backlinks.GetBacklinksForNote(fileName);
            if (noteBacklinks.Any())
            {
                htmlContent += "<h2>Backlinks</h2><ul>";
                foreach (var link in noteBacklinks)
                {
                    htmlContent += $"<li><a href=\"{link}.html\">{link}</a></li>";
                }
                htmlContent += "</ul>";
            }

            var renderedContent = _templateRenderer.RenderNote(
                new NoteModel(
                    htmlContent,
                    noteBacklinks.Select(b => new BacklinkModel(b + ".html", b, "")).ToList()
                ),
                new LayoutModel("SomeTitle", "SomeDescription", "Website", "SomeUrl", null)
            );

            var outputFile = Path.Combine(outputPath, $"{fileName}.html");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
            await File.WriteAllTextAsync(outputFile, renderedContent);
        }
    }
}
