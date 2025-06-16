using FluentAssertions;
using SiteGenerator.KnowledgeGraph;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests.KnowledgeGraph;

public class GraphBuilderTests
{
    [Fact]
    public async Task BuildGraphAsync_WithValidContent_CreatesValidGraph()
    {
        // Arrange
        var fileProvider = new InMemoryFileProvider();
        var markdownParser = new MarkdownParser();
        var graphBuilder = new GraphBuilder(fileProvider, markdownParser);

        // Add test content with valid links
        fileProvider.AddFile("note1.md", "# Note 1\nThis links to [[note2]]");
        fileProvider.AddFile("note2.md", "# Note 2\nThis links back to [[note1]]");
        fileProvider.AddFile("note3.md", "# Note 3\nStandalone note");

        // Act
        var result = await graphBuilder.BuildGraphAsync("/test");

        // Assert
        result.Should().NotBeNull();
        result.Nodes.Should().HaveCount(3);
        result.Links.Should().HaveCount(2); // note1->note2, note2->note1

        // Verify all links reference existing nodes
        var nodeIds = result.Nodes.Select(n => n.Id).ToHashSet();
        foreach (var link in result.Links)
        {
            nodeIds
                .Should()
                .Contain(link.Source, "all link sources should reference existing nodes");
            nodeIds
                .Should()
                .Contain(link.Target, "all link targets should reference existing nodes");
        }
    }

    [Fact]
    public async Task BuildGraphAsync_WithInvalidLinks_FiltersInvalidLinks()
    {
        // Arrange
        var fileProvider = new InMemoryFileProvider();
        var markdownParser = new MarkdownParser();
        var graphBuilder = new GraphBuilder(fileProvider, markdownParser);

        // Add content with invalid link (missing-note doesn't exist)
        fileProvider.AddFile("note1.md", "# Note 1\nThis links to [[missing-note]] and [[note2]]");
        fileProvider.AddFile("note2.md", "# Note 2\nValid note");

        // Act
        var result = await graphBuilder.BuildGraphAsync("/test");

        // Assert
        result.Should().NotBeNull();
        result.Nodes.Should().HaveCount(2);

        // Should only have valid links
        result.Links.Should().HaveCount(1);
        result.Links.Should().Contain(l => l.Source == "note1" && l.Target == "note2");

        // Verify no invalid links exist
        var nodeIds = result.Nodes.Select(n => n.Id).ToHashSet();
        foreach (var link in result.Links)
        {
            nodeIds.Should().Contain(link.Source);
            nodeIds.Should().Contain(link.Target);
        }
    }

    [Fact]
    public async Task BuildGraphAsync_ClassifiesNodeTypes()
    {
        // Arrange
        var fileProvider = new InMemoryFileProvider();
        var markdownParser = new MarkdownParser();
        var graphBuilder = new GraphBuilder(fileProvider, markdownParser);

        // Create a hub node (many incoming links)
        fileProvider.AddFile("hub.md", "# Hub Note");
        fileProvider.AddFile("note1.md", "# Note 1\nLinks to [[hub]]");
        fileProvider.AddFile("note2.md", "# Note 2\nLinks to [[hub]]");
        fileProvider.AddFile("note3.md", "# Note 3\nLinks to [[hub]]");
        fileProvider.AddFile("note4.md", "# Note 4\nLinks to [[hub]]");
        fileProvider.AddFile("standalone.md", "# Standalone\nNo links");

        // Act
        var result = await graphBuilder.BuildGraphAsync("/test");

        // Assert
        var hubNode = result.Nodes.Should().ContainSingle(n => n.Id == "hub").Subject;
        hubNode.Type.Should().Be(NodeType.Hub, "nodes with 4+ incoming links should be hubs");

        var standaloneNode = result.Nodes.Should().ContainSingle(n => n.Id == "standalone").Subject;
        standaloneNode
            .Type.Should()
            .Be(NodeType.Note, "nodes with no links should be regular notes");
    }

    [Fact]
    public async Task BuildGraphAsync_ExtractsObsidianStyleLinks()
    {
        // Arrange
        var fileProvider = new InMemoryFileProvider();
        var markdownParser = new MarkdownParser();
        var graphBuilder = new GraphBuilder(fileProvider, markdownParser);

        fileProvider.AddFile(
            "source.md",
            "# Source\nObsidian link: [[target]]\nMarkdown link: [Target](/notes/target/)"
        );
        fileProvider.AddFile("target.md", "# Target");

        // Act
        var result = await graphBuilder.BuildGraphAsync("/test");

        // Assert
        result.Links.Should().HaveCount(2);
        result
            .Links.Should()
            .Contain(l =>
                l.Source == "source" && l.Target == "target" && l.Type == LinkType.Reference
            );
    }

    [Fact]
    public async Task BuildGraphAsync_CategorizesNodes()
    {
        // Arrange
        var fileProvider = new InMemoryFileProvider();
        var markdownParser = new MarkdownParser();
        var graphBuilder = new GraphBuilder(fileProvider, markdownParser);

        fileProvider.AddFile("sql-tips.md", "# SQL Tips\nDatabase content with SQL");
        fileProvider.AddFile("csharp-guide.md", "# C# Guide\nProgramming content");
        fileProvider.AddFile("career-advice.md", "# Career Advice\nCareer content");
        fileProvider.AddFile("random-note.md", "# Random Note\nGeneral content");

        // Act
        var result = await graphBuilder.BuildGraphAsync("/test");

        // Assert
        result.Nodes.Should().ContainSingle(n => n.Id == "sql-tips" && n.Category == "Database");
        result
            .Nodes.Should()
            .ContainSingle(n => n.Id == "csharp-guide" && n.Category == "Programming");
        result.Nodes.Should().ContainSingle(n => n.Id == "career-advice" && n.Category == "Career");
        result.Nodes.Should().ContainSingle(n => n.Id == "random-note" && n.Category == "General");
    }
}
