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
    private readonly GitHistoryService _gitHistoryService;

    public NoteProcessor(
        Backlinks backlinks,
        TemplateRenderer templateRenderer,
        IFileProvider folderReader,
        MarkdownParser markdownParser,
        SiteMetadata config,
        GraphData? graphData = null,
        GitHistoryService? gitHistoryService = null
    )
    {
        _backlinks = backlinks;
        _templateRenderer = templateRenderer;
        _folderReader = folderReader;
        _markdownParser = markdownParser;
        _config = config;
        _graphData = graphData;
        _gitHistoryService = gitHistoryService ?? new GitHistoryService(".");
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        // Load all markdown files into memory
        var allNotesContent = await LoadAllMarkdownFilesAsync(inputPath);

        // Generate previews for all notes
        var notePreviews = GenerateNotePreviews(allNotesContent);

        // Generate recent notes data once for all notes
        var recentNotesModel = await GenerateRecentNotesAsync(allNotesContent);

        // Process each note using the in-memory content, previews, and shared recent notes data
        foreach (var kvp in allNotesContent)
        {
            var fileName = kvp.Key;
            var markdownContent = kvp.Value;

            var htmlContent = _markdownParser.ParseToHtml(markdownContent);
            var renderedContent = RenderNoteWithTemplate(
                htmlContent,
                fileName,
                notePreviews,
                recentNotesModel
            );

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
            var htmlContent = _markdownParser.ParseToHtml(markdownContent);

            // Generate preview
            var previewHtml = PreviewGenerator.GeneratePreview(htmlContent);

            // Store the preview
            previews[fileName] = previewHtml;
        }

        return previews;
    }

    private async Task<RecentNotesModel> GenerateRecentNotesAsync(
        Dictionary<string, string> allNotesContent
    )
    {
        try
        {
            // Get recent notes from git history
            var newestNotesFileNames = await _gitHistoryService.GetRecentNotesAsync(5);
            var recentlyChangedFileNames = await _gitHistoryService.GetRecentlyChangedNotesAsync(
                10
            ); // Get more to account for filtering

            // Convert newest notes to RecentNoteItem objects
            var newestNotes = newestNotesFileNames
                .Where(fileName => allNotesContent.ContainsKey(fileName))
                .Select(fileName => new RecentNoteItem(
                    fileName,
                    ExtractTitle(_markdownParser.ParseToHtml(allNotesContent[fileName]))
                        ?? FormatFileName(fileName)
                ))
                .ToList();

            // Create set of newest note file names for exclusion
            var newestNotesSet = newestNotesFileNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Filter recently changed to exclude newest notes and convert to RecentNoteItem objects
            var recentlyChangedNotes = recentlyChangedFileNames
                .Where(fileName =>
                    allNotesContent.ContainsKey(fileName) && !newestNotesSet.Contains(fileName)
                ) // Exclude files that are in newest notes
                .Select(fileName => new RecentNoteItem(
                    fileName,
                    ExtractTitle(_markdownParser.ParseToHtml(allNotesContent[fileName]))
                        ?? FormatFileName(fileName)
                ))
                .Take(5) // Take only 5 after filtering
                .ToList();

            return new RecentNotesModel(newestNotes, recentlyChangedNotes);
        }
        catch
        {
            // If git history fails, return empty model
            return new RecentNotesModel();
        }
    }

    private string RenderNoteWithTemplate(
        string htmlContent,
        string fileName,
        Dictionary<string, string> notePreviews,
        RecentNotesModel recentNotesModel
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

        var noteModel = new NoteModel(htmlContent, backlinks, noteGraphData, recentNotesModel);
        var pageUrl = $"{_config.BaseUrl}/notes/{fileName}/";

        // Extract title from first header in the content
        var titleName = ExtractTitle(htmlContent) ?? FormatFileName(fileName);
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
        return h1Match.Success ? h1Match.Groups[1].Value : null;
    }

    private static string FormatFileName(string fileName, char join = ' ')
    {
        // Fallback if no h1 is found - more sophisticated filename formatting
        return string.Join(
            join,
            fileName.Split('-', '_').Select(word => char.ToUpper(word[0]) + word[1..])
        );
    }

    private async Task SaveNoteToFile(string content, string fileName, string outputPath)
    {
        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        await _folderReader.WriteFileAsync(outputFile, content);
    }
}
