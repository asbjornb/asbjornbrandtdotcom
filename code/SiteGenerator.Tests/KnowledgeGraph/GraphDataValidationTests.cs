using System.Text.Json;
using FluentAssertions;
using SiteGenerator.KnowledgeGraph;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests.KnowledgeGraph;

public class GraphDataValidationTests : IClassFixture<SiteGenerationFixture>
{
    private readonly SiteGenerationFixture _fixture;

    public GraphDataValidationTests(SiteGenerationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void GeneratedGraphData_ShouldHaveValidStructure()
    {
        // Arrange
        var graphDataPath = Path.Combine(
            TestConstants.ActualOutputPath,
            "assets",
            "graph-data.json"
        );

        // Act & Assert
        File.Exists(graphDataPath).Should().BeTrue("graph-data.json should be generated");

        var jsonContent = File.ReadAllText(graphDataPath);
        var graphData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        // Verify required properties exist
        graphData.TryGetProperty("nodes", out var nodesProperty).Should().BeTrue();
        graphData.TryGetProperty("links", out var linksProperty).Should().BeTrue();
        graphData.TryGetProperty("categories", out var categoriesProperty).Should().BeTrue();
        graphData.TryGetProperty("stats", out var statsProperty).Should().BeTrue();

        var nodes = nodesProperty.EnumerateArray().ToList();
        var links = linksProperty.EnumerateArray().ToList();

        // Basic validation
        nodes.Should().NotBeEmpty("should have at least one node");

        // Validate node structure
        foreach (var node in nodes)
        {
            node.TryGetProperty("id", out _).Should().BeTrue("each node should have an id");
            node.TryGetProperty("title", out _).Should().BeTrue("each node should have a title");
            node.TryGetProperty("url", out _).Should().BeTrue("each node should have a url");
            node.TryGetProperty("category", out _)
                .Should()
                .BeTrue("each node should have a category");
            node.TryGetProperty("type", out _).Should().BeTrue("each node should have a type");
        }

        // Validate link structure and references
        var nodeIds = nodes.Select(n => n.GetProperty("id").GetString()).ToHashSet();

        foreach (var link in links)
        {
            link.TryGetProperty("source", out var sourceProperty)
                .Should()
                .BeTrue("each link should have a source");
            link.TryGetProperty("target", out var targetProperty)
                .Should()
                .BeTrue("each link should have a target");
            link.TryGetProperty("type", out var typeProperty)
                .Should()
                .BeTrue("each link should have a type");

            var source = sourceProperty.GetString();
            var target = targetProperty.GetString();

            nodeIds
                .Should()
                .Contain(source, $"link source '{source}' should reference an existing node");
            nodeIds
                .Should()
                .Contain(target, $"link target '{target}' should reference an existing node");
        }

        // Validate stats
        statsProperty.TryGetProperty("totalNodes", out var totalNodesProperty).Should().BeTrue();
        statsProperty.TryGetProperty("totalLinks", out var totalLinksProperty).Should().BeTrue();

        var totalNodes = totalNodesProperty.GetInt32();
        var totalLinks = totalLinksProperty.GetInt32();

        totalNodes.Should().Be(nodes.Count, "stats should match actual node count");
        totalLinks.Should().Be(links.Count, "stats should match actual link count");
    }

    [Fact]
    public void GraphData_ShouldNotContainOrphanedLinks()
    {
        // Arrange
        var graphDataPath = Path.Combine(
            TestConstants.ActualOutputPath,
            "assets",
            "graph-data.json"
        );

        // Act
        var jsonContent = File.ReadAllText(graphDataPath);
        var graphData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        var nodes = graphData.GetProperty("nodes").EnumerateArray().ToList();
        var links = graphData.GetProperty("links").EnumerateArray().ToList();

        var nodeIds = nodes.Select(n => n.GetProperty("id").GetString()).ToHashSet();

        // Assert
        foreach (var link in links)
        {
            var source = link.GetProperty("source").GetString();
            var target = link.GetProperty("target").GetString();

            nodeIds
                .Should()
                .Contain(source, $"Link source '{source}' should reference an existing node");
            nodeIds
                .Should()
                .Contain(target, $"Link target '{target}' should reference an existing node");
        }
    }

    [Fact]
    public void GraphData_ShouldHaveValidNodeTypes()
    {
        // Arrange
        var graphDataPath = Path.Combine(
            TestConstants.ActualOutputPath,
            "assets",
            "graph-data.json"
        );
        var validNodeTypes = new[] { "note", "category", "hub" };

        // Act
        var jsonContent = File.ReadAllText(graphDataPath);
        var graphData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
        var nodes = graphData.GetProperty("nodes").EnumerateArray().ToList();

        // Assert
        foreach (var node in nodes)
        {
            var nodeType = node.GetProperty("type").GetString();
            validNodeTypes.Should().Contain(nodeType, $"Node type '{nodeType}' should be valid");
        }
    }

    [Fact]
    public void GraphData_ShouldHaveValidLinkTypes()
    {
        // Arrange
        var graphDataPath = Path.Combine(
            TestConstants.ActualOutputPath,
            "assets",
            "graph-data.json"
        );
        var validLinkTypes = new[] { "reference", "hierarchical", "related", "external" };

        // Act
        var jsonContent = File.ReadAllText(graphDataPath);
        var graphData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
        var links = graphData.GetProperty("links").EnumerateArray().ToList();

        // Assert
        foreach (var link in links)
        {
            var linkType = link.GetProperty("type").GetString();
            validLinkTypes.Should().Contain(linkType, $"Link type '{linkType}' should be valid");
        }
    }
}
