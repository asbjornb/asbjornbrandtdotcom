using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using Xunit;

namespace SiteGenerator.Tests.Integration;

public sealed class PublishedNotesIntegrationTests : IAsyncLifetime
{
    private const string InputPath = "TestData/PublishedNotesTestInput";
    private const string OutputPath = "TestOutput_PublishedNotes";
    private readonly Generator _generator;

    public PublishedNotesIntegrationTests()
    {
        var siteMetadata =
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("SiteMetadata")
                .Get<SiteMetadata>()
            ?? throw new Exception("Could not bind configuration sections to records.");

        var templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
        _generator = new Generator(InputPath, OutputPath, templateRenderer, siteMetadata);
    }

    public async Task InitializeAsync()
    {
        if (Directory.Exists(OutputPath))
        {
            Directory.Delete(OutputPath, true);
        }
        Directory.CreateDirectory(OutputPath);

        await _generator.GenerateSiteAsync();
    }

    public Task DisposeAsync()
    {
        try
        {
            Directory.Delete(OutputPath, true);
        }
        catch
        {
            // Ignore cleanup failures
        }
        return Task.CompletedTask;
    }

    private static string ReadOutput(params string[] path) =>
        File.ReadAllText(Path.Combine([OutputPath, .. path]));

    [Fact]
    public void OnlyPublishedNotesAreRendered()
    {
        File.Exists(Path.Combine(OutputPath, "notes", "alpha", "index.html")).Should().BeTrue();
        File.Exists(Path.Combine(OutputPath, "notes", "beta", "index.html")).Should().BeTrue();
        File.Exists(Path.Combine(OutputPath, "notes", "index.html")).Should().BeTrue();
        Directory.Exists(Path.Combine(OutputPath, "notes", "draft")).Should().BeFalse();
    }

    [Fact]
    public void WikilinksToUnpublishedNotesRenderAsPlainText()
    {
        var alpha = ReadOutput("notes", "alpha", "index.html");
        alpha.Should().Contain("href=\"/notes/beta/\"");
        alpha.Should().NotContain("/notes/draft/");
        alpha.Should().Contain("and draft.");

        var notesIndex = ReadOutput("notes", "index.html");
        notesIndex.Should().NotContain("/notes/draft/");

        var home = ReadOutput("index.html");
        home.Should().Contain("href=\"/notes/alpha/\"");
        home.Should().NotContain("/notes/draft/");
    }

    [Fact]
    public void BacklinksOnlyComeFromPublishedNotes()
    {
        // draft links to alpha, but is unpublished, so alpha must not show it as a backlink
        var alpha = ReadOutput("notes", "alpha", "index.html");
        alpha.Should().Contain("/notes/beta/");
        alpha.Should().NotContain("Draft");
    }

    [Fact]
    public void KnowledgeGraphOnlyContainsPublishedNotes()
    {
        var graphJson = ReadOutput("assets", "graph-data.json");
        graphJson.Should().Contain("\"id\": \"alpha\"");
        graphJson.Should().NotContain("draft");
    }

    [Fact]
    public void ReportsLinksToUnpublishedNotes()
    {
        _generator.LinksToUnpublishedNotes.Keys.Should().BeEquivalentTo(["draft"]);
        _generator
            .LinksToUnpublishedNotes["draft"]
            .Should()
            .BeEquivalentTo(["index", "notes/alpha", "notes/index"]);
    }
}
