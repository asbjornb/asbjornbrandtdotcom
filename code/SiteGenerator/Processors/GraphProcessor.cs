using System.Text.Json;
using SiteGenerator.Configuration;
using SiteGenerator.KnowledgeGraph;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class GraphProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _fileProvider;
    private readonly MarkdownParser _markdownParser;
    private readonly SiteMetadata _siteMetadata;

    public GraphProcessor(
        TemplateRenderer templateRenderer,
        IFileProvider fileProvider,
        MarkdownParser markdownParser,
        SiteMetadata siteMetadata
    )
    {
        _templateRenderer = templateRenderer;
        _fileProvider = fileProvider;
        _markdownParser = markdownParser;
        _siteMetadata = siteMetadata;
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        var graphBuilder = new GraphBuilder(_fileProvider, _markdownParser);
        var graphData = await graphBuilder.BuildGraphAsync(inputPath);

        // Generate the graph data as JSON for the frontend
        await CreateGraphDataFile(graphData, outputPath);
    }

    public async Task<GraphData> GetGraphDataAsync(string inputPath)
    {
        var graphBuilder = new GraphBuilder(_fileProvider, _markdownParser);
        return await graphBuilder.BuildGraphAsync(inputPath);
    }

    private async Task CreateGraphDataFile(GraphData graphData, string outputPath)
    {
        // Convert to a format suitable for D3.js
        var d3Data = new
        {
            nodes = graphData.Nodes.Select(n => new
            {
                id = n.Id,
                title = n.Title,
                url = n.Url,
                category = n.Category,
                size = n.Size,
                headers = n.Headers,
                type = n.Type.ToString().ToLower(),
                group = GetCategoryGroup(n.Category),
            }),
            links = graphData.Links.Select(l => new
            {
                source = l.Source,
                target = l.Target,
                type = l.Type.ToString().ToLower(),
                strength = l.Strength,
            }),
            categories = graphData.Categories,
            stats = new
            {
                totalNodes = graphData.Nodes.Count,
                totalLinks = graphData.Links.Count,
                hubNodes = graphData.Nodes.Count(n => n.Type == NodeType.Hub),
                categoryNodes = graphData.Nodes.Count(n => n.Type == NodeType.Category),
            },
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        var jsonContent = JsonSerializer.Serialize(d3Data, jsonOptions);

        await _fileProvider.WriteFileAsync(
            Path.Combine(outputPath, "assets", "graph-data.json"),
            jsonContent
        );
    }

    private static int GetCategoryGroup(string category)
    {
        return category switch
        {
            "Database" => 1,
            "Programming" => 2,
            "Career" => 3,
            "Tools" => 4,
            "Organization" => 5,
            _ => 0,
        };
    }
}
