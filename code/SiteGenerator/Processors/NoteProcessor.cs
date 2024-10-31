using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Previews;
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
        // Load all markdown files into memory
        var allNotesContent = await LoadAllMarkdownFilesAsync(inputPath);

        // Generate previews for all notes
        var notePreviews = GenerateNotePreviews(allNotesContent);

        // Process each note using the in-memory content and previews
        foreach (var kvp in allNotesContent)
        {
            var fileName = kvp.Key;
            var markdownContent = kvp.Value;

            var htmlContent = _markdownConverter.ConvertToHtml(markdownContent);
            var renderedContent = RenderNoteWithTemplate(htmlContent, fileName, notePreviews);

            if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                await SaveNoteToFile(renderedContent, fileName, outputPath);
            }
            else
            {
                var noteFolder = Path.Combine(outputPath, fileName);
                await SaveNoteToFile(renderedContent, "index", noteFolder);
            }
        }
    }

    private async Task<Dictionary<string, string>> LoadAllMarkdownFilesAsync(string inputPath)
    {
        var files = new Dictionary<string, string>();

        await foreach (var contentFile in _folderReader.GetFileContents(inputPath, "*.md"))
        {
            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            files[fileName] = contentFile.Content;
        }

        return files;
    }

    private Dictionary<string, string> GenerateNotePreviews(
        Dictionary<string, string> allNotesContent
    )
    {
        var previews = new Dictionary<string, string>();

        foreach (var kvp in allNotesContent)
        {
            var fileName = kvp.Key;
            var markdownContent = kvp.Value;

            // Convert markdown to HTML
            var htmlContent = _markdownConverter.ConvertToHtml(markdownContent);

            // Generate preview
            var previewHtml = PreviewGenerator.GeneratePreview(htmlContent);

            // Store the preview
            previews[fileName] = previewHtml;
        }

        return previews;
    }

    private string RenderNoteWithTemplate(
        string htmlContent,
        string fileName,
        Dictionary<string, string> notePreviews
    )
    {
        var backlinksList = _backlinks.GetBacklinksForNote(fileName);

        var backlinks = backlinksList
            .Select(backlinkFileName =>
            {
                // Retrieve the preview from the dictionary
                if (notePreviews.TryGetValue(backlinkFileName, out var previewHtml))
                {
                    return new BacklinkModel(
                        Url: $"/notes/{backlinkFileName}/",
                        Title: char.ToUpper(backlinkFileName[0]) + backlinkFileName[1..],
                        PreviewHtml: previewHtml
                    );
                }
                else
                {
                    // This shouldn't happen since we assume all backlinks exist
                    // But you can handle this case if necessary
                    return null;
                }
            })
            .Where(backlink => backlink != null)
            .ToList();

        var noteModel = new NoteModel(htmlContent, backlinks);
        var pageUrl = $"{_config.BaseUrl}/notes/{fileName}/";

        var titleName = char.ToUpper(fileName[0]) + fileName[1..];
        var pageTitle = $"{titleName} • {_config.Author}'s Notes";

        var layoutModel = new LayoutModel(pageTitle, _config.Description, "article", pageUrl, null);

        return _templateRenderer.RenderNote(noteModel, layoutModel);
    }

    private async Task SaveNoteToFile(string content, string fileName, string outputPath)
    {
        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        await _folderReader.WriteFileAsync(outputFile, content);
    }
}
