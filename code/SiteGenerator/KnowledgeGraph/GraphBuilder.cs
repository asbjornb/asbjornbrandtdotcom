using System.Text.RegularExpressions;
using SiteGenerator.BacklinksProcessing;

namespace SiteGenerator.KnowledgeGraph;

public class GraphBuilder
{
    private readonly IFileProvider _fileProvider;
    private readonly MarkdownParser _markdownParser;

    public GraphBuilder(IFileProvider fileProvider, MarkdownParser markdownParser)
    {
        _fileProvider = fileProvider;
        _markdownParser = markdownParser;
    }

    public async Task<GraphData> BuildGraphAsync(string contentPath)
    {
        var nodes = new List<GraphNode>();
        var links = new List<GraphLink>();
        var categories = new Dictionary<string, int>();

        // Collect all notes and analyze their content
        await foreach (var contentFile in _fileProvider.GetFileContents(contentPath, "*.md"))
        {
            var node = CreateNodeFromContent(contentFile, contentPath);
            nodes.Add(node);

            // Count categories
            if (categories.ContainsKey(node.Category))
                categories[node.Category]++;
            else
                categories[node.Category] = 1;

            // Extract links from content
            var nodeLinks = ExtractLinksFromContent(contentFile.Content, contentFile.Name);
            links.AddRange(nodeLinks);
        }

        // Filter out links to non-existent nodes
        var validNodeIds = nodes.Select(n => n.Id).ToHashSet();
        var validLinks = links
            .Where(l => validNodeIds.Contains(l.Source) && validNodeIds.Contains(l.Target))
            .ToList();

        // Classify hub nodes based on connectivity
        ClassifyHubNodes(nodes, validLinks);

        // Add hierarchical links based on categories
        AddHierarchicalLinks(nodes, validLinks);

        return new GraphData(nodes, validLinks, categories);
    }

    private GraphNode CreateNodeFromContent(ContentFile contentFile, string contentPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
        var htmlContent = _markdownParser.ParseToHtml(contentFile.Content);

        // Extract title from first H1 or use filename
        var title = ExtractTitle(htmlContent) ?? FormatTitle(fileName);

        // Extract all headers
        var headers = ExtractHeaders(htmlContent);

        // Determine category (could be enhanced with more sophisticated logic)
        var category = DetermineCategory(fileName, contentFile.Content);

        // Calculate size based on content length and complexity
        var size = CalculateNodeSize(contentFile.Content, headers.Count);

        var url = $"/notes/{fileName}/";

        return new GraphNode(fileName, title, url, category, size, headers);
    }

    private List<GraphLink> ExtractLinksFromContent(string content, string sourceFileName)
    {
        var links = new List<GraphLink>();
        var sourceId = Path.GetFileNameWithoutExtension(sourceFileName);

        // Extract Obsidian-style [[links]]
        var obsidianLinks = Regex.Matches(content, @"\[\[([^\]]+)\]\]");
        foreach (Match match in obsidianLinks)
        {
            var targetId = match.Groups[1].Value.Trim();
            links.Add(new GraphLink(sourceId, targetId, LinkType.Reference, 2));
        }

        // Extract regular markdown links to internal content
        var markdownLinks = Regex.Matches(content, @"\[([^\]]+)\]\(([^)]+)\)");
        foreach (Match match in markdownLinks)
        {
            var url = match.Groups[2].Value;
            if (url.StartsWith("/notes/") && url.EndsWith("/"))
            {
                var targetId = url.Replace("/notes/", "").Replace("/", "");
                links.Add(new GraphLink(sourceId, targetId, LinkType.Reference, 1));
            }
        }

        return links;
    }

    private void ClassifyHubNodes(List<GraphNode> nodes, List<GraphLink> links)
    {
        // Count incoming links for each node
        var incomingCounts = links.GroupBy(l => l.Target).ToDictionary(g => g.Key, g => g.Count());

        // Classify nodes based on connectivity and create updated list
        for (int i = 0; i < nodes.Count; i++)
        {
            var node = nodes[i];
            var incomingCount = incomingCounts.GetValueOrDefault(node.Id, 0);
            var nodeType = incomingCount switch
            {
                >= 4 => NodeType.Hub,
                >= 2 => NodeType.Category,
                _ => NodeType.Note,
            };

            // Update node with new type
            nodes[i] = node with
            {
                Type = nodeType,
            };
        }
    }

    private void AddHierarchicalLinks(List<GraphNode> nodes, List<GraphLink> links)
    {
        // Add hierarchical relationships for category nodes
        var categoryNodes = nodes.Where(n => n.Type == NodeType.Category).ToList();
        var hubNodes = nodes.Where(n => n.Type == NodeType.Hub).ToList();

        // Connect hubs to their related categories
        foreach (var hub in hubNodes)
        {
            var relatedCategories = categoryNodes.Where(c => IsRelated(hub, c)).ToList();

            foreach (var category in relatedCategories)
            {
                links.Add(new GraphLink(hub.Id, category.Id, LinkType.Hierarchical, 3));
            }
        }
    }

    private static string ExtractTitle(string htmlContent)
    {
        var match = Regex.Match(htmlContent, @"<h1[^>]*>(.*?)</h1>", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static List<string> ExtractHeaders(string htmlContent)
    {
        var headers = new List<string>();
        var matches = Regex.Matches(
            htmlContent,
            @"<h([2-6])[^>]*>(.*?)</h\1>",
            RegexOptions.IgnoreCase
        );

        foreach (Match match in matches)
        {
            var level = int.Parse(match.Groups[1].Value);
            var text = match.Groups[2].Value.Trim();
            headers.Add($"H{level}: {text}");
        }

        return headers;
    }

    private static string FormatTitle(string fileName)
    {
        return fileName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(' ')
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower())
            .Aggregate((a, b) => $"{a} {b}");
    }

    private static string DetermineCategory(string fileName, string content)
    {
        // Simple category classification based on filename and content
        if (fileName.Contains("sql") || fileName.Contains("database") || content.Contains("SQL"))
            return "Database";
        if (fileName.Contains("csharp") || fileName.Contains("python") || fileName.Contains("git"))
            return "Programming";
        if (fileName.Contains("career") || fileName.Contains("work") || fileName.Contains("staff"))
            return "Career";
        if (
            fileName.Contains("shortcut")
            || fileName.Contains("tool")
            || fileName.Contains("cheat")
        )
            return "Tools";
        if (fileName == "index" || fileName.Contains("inbox"))
            return "Organization";

        return "General";
    }

    private static int CalculateNodeSize(string content, int headerCount)
    {
        // Base size on content length and structure complexity
        var contentLength = content.Length;
        var baseSize = Math.Min(contentLength / 50, 100); // Scale content length
        var headerBonus = headerCount * 5; // Bonus for structured content

        return Math.Max(10, Math.Min(baseSize + headerBonus, 150)); // Clamp between 10-150
    }

    private static bool IsRelated(GraphNode hub, GraphNode category)
    {
        // Simple relatedness check - could be enhanced with more sophisticated logic
        return hub.Category == category.Category
            || hub.Id.Contains(category.Id)
            || category.Id.Contains(hub.Id);
    }
}
