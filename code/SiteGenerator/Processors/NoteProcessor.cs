using System.Text.RegularExpressions;
using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Configuration;
using SiteGenerator.KnowledgeGraph;
using SiteGenerator.Previews;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class NoteProcessor : IPageProcessor
{
    private readonly Backlinks _backlinks;
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _folderReader;
    private readonly MarkdownParser _markdownParser;
    private readonly SiteMetadata _config;
    private readonly GraphData? _graphData;

    public NoteProcessor(
        Backlinks backlinks,
        TemplateRenderer templateRenderer,
        IFileProvider folderReader,
        MarkdownParser markdownParser,
        SiteMetadata config,
        GraphData? graphData = null
    )
    {
        _backlinks = backlinks;
        _templateRenderer = templateRenderer;
        _folderReader = folderReader;
        _markdownParser = markdownParser;
        _config = config;
        _graphData = graphData;
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

            var htmlContent = _markdownParser.ParseToHtml(markdownContent);
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
            WarnIfSlugHasConsecutiveSeparators(fileName);
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
            var htmlContent = _markdownParser.ParseToHtml(markdownContent);

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

        List<BacklinkModel> backlinks = backlinksList
            .Select(backlinkFileName =>
            {
                // Retrieve the preview from the dictionary
                if (notePreviews.TryGetValue(backlinkFileName, out var previewHtml))
                {
                    return new BacklinkModel(
                        Url: $"/notes/{backlinkFileName}/",
                        Title: FormatFileName(backlinkFileName, '-'),
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
            .ToList()!;

        // Extract graph data for the current note
        var noteGraphData = ExtractNoteGraphData(fileName);

        var noteModel = new NoteModel(htmlContent, backlinks, noteGraphData);
        var pageUrl = $"{_config.BaseUrl}/notes/{fileName}/";

        // Extract title from first header in the content
        var extractedTitle = ExtractTitle(htmlContent);
        if (string.IsNullOrEmpty(extractedTitle))
        {
            GenerationWarnings.NoteMissingTitle(fileName);
        }

        var titleName = extractedTitle ?? FormatFileName(fileName);
        var pageTitle = $"{titleName} • {_config.Author}'s Notes";

        var layoutModel = new LayoutModel(pageTitle, _config.Description, "article", pageUrl, null);

        return _templateRenderer.RenderNote(noteModel, layoutModel);
    }

    private NoteGraphData? ExtractNoteGraphData(string fileName)
    {
        if (_graphData == null)
            return null;

        // Find the current note in the graph data
        var currentNode = _graphData.Nodes.FirstOrDefault(n => n.Id == fileName);
        if (currentNode == null)
            return null;

        // Find all connections to/from this note
        var connections = _graphData
            .Links.Where(l => l.Source == fileName || l.Target == fileName)
            .ToList();

        // Find all connected nodes
        var connectedNodeIds = connections
            .Select(l => l.Source == fileName ? l.Target : l.Source)
            .Distinct()
            .ToList();

        var connectedNodes = _graphData.Nodes.Where(n => connectedNodeIds.Contains(n.Id)).ToList();

        return new NoteGraphData(
            fileName,
            connectedNodes,
            connections,
            currentNode.Category,
            currentNode.Type
        );
    }

    private static string? ExtractTitle(string htmlContent)
    {
        // Look for first h1 tag
        var h1Match = Regex.Match(htmlContent, @"<h1>(.*?)</h1>");
        if (!h1Match.Success)
            return null;

        var value = h1Match.Groups[1].Value.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string FormatFileName(string fileName, char join = ' ')
    {
        var segments = fileName
            .Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => segment.Trim())
            .Where(segment => segment.Length > 0)
            .Select(Capitalise);

        var formatted = string.Join(join, segments);

        return string.IsNullOrEmpty(formatted) ? fileName : formatted;
    }

    private async Task SaveNoteToFile(string content, string fileName, string outputPath)
    {
        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        await _folderReader.WriteFileAsync(outputFile, content);
    }

    private static void WarnIfSlugHasConsecutiveSeparators(string fileName)
    {
        if (
            fileName.Contains("--", StringComparison.Ordinal)
            || fileName.Contains("__", StringComparison.Ordinal)
        )
        {
            GenerationWarnings.NoteSlugHasDoubleSeparator(fileName);
        }
    }

    private static string Capitalise(string segment)
    {
        if (segment.Length == 0)
            return segment;

        if (segment.Length == 1)
            return char.ToUpperInvariant(segment[0]).ToString();

        return char.ToUpperInvariant(segment[0]) + segment[1..];
    }
}
