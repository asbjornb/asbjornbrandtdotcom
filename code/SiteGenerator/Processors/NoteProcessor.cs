using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class NoteProcessor : IPageProcessor
{
    private readonly Backlinks _backlinks;
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _folderReader;
    private readonly IMarkdownConverter _markdownConverter;
    private readonly Config _config;

    public NoteProcessor(
        Backlinks backlinks,
        TemplateRenderer templateRenderer,
        IFileProvider folderReader,
        IMarkdownConverter markdownConverter,
        Config config
    )
    {
        _backlinks = backlinks;
        _templateRenderer = templateRenderer;
        _folderReader = folderReader;
        _markdownConverter = markdownConverter;
        _config = config;
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        await foreach (var contentFile in _folderReader.GetFileContents(inputPath, "*.md"))
        {
            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var htmlContent = _markdownConverter.ConvertToHtml(contentFile.Content);
            var processedHtml = AddBacklinksToHtml(htmlContent, fileName);
            var renderedContent = RenderNoteWithTemplate(processedHtml, fileName);

            if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                // Put index.html directly in the output folder
                await SaveNoteToFile(renderedContent, fileName, outputPath);
            }
            else
            {
                // Create a folder for each non-index note and put an index.html inside it
                var noteFolder = Path.Combine(outputPath, fileName);
                await SaveNoteToFile(renderedContent, "index", noteFolder);
            }
        }
    }

    private string AddBacklinksToHtml(string html, string fileName)
    {
        var noteBacklinks = _backlinks.GetBacklinksForNote(fileName);
        if (!noteBacklinks.Any())
            return html;

        return html
            + "<h2>Backlinks</h2><ul>"
            + string.Join(
                "",
                noteBacklinks.Select(link => $"<li><a href=\"/{link}/\">/{link}/</a></li>")
            )
            + "</ul>";
    }

    private string RenderNoteWithTemplate(string htmlContent, string fileName)
    {
        var backlinks = _backlinks
            .GetBacklinksForNote(fileName)
            .Select(b => new BacklinkModel($"/{b}/", b, ""))
            .ToList();

        var noteModel = new NoteModel(htmlContent, backlinks);
        var layoutModel = new LayoutModel(
            "SomeTitle",
            "SomeDescription",
            "Website",
            "SomeUrl",
            null
        );

        return _templateRenderer.RenderNote(noteModel, layoutModel);
    }

    private async Task SaveNoteToFile(string content, string fileName, string outputPath)
    {
        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        await _folderReader.WriteFileAsync(outputFile, content);
    }
}
