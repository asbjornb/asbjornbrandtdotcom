using System.Text.Json;
using SiteGenerator.Tests.TemplateTests;
using Xunit;

namespace SiteGenerator.Tests;

public class GeneratorExplorationTests : IAsyncLifetime
{
    private string _testRootPath = null!;
    private string _contentPath = null!;
    private string _outputPath = null!;
    private string _configPath = null!;

    public Task InitializeAsync()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _contentPath = Path.Combine(_testRootPath, "TestContent");
        _outputPath = Path.Combine(_testRootPath, "TestOutput");
        _configPath = Path.Combine(_testRootPath, "config.json");

        Directory.CreateDirectory(_contentPath);
        Directory.CreateDirectory(_outputPath);
        Directory.CreateDirectory(Path.Combine(_contentPath, "pages"));
        Directory.CreateDirectory(Path.Combine(_contentPath, "thoughts"));
        Directory.CreateDirectory(Path.Combine(_contentPath, "posts"));

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GenerateSiteAsync_ShouldCreateHtmlFiles()
    {
        // Arrange
        await CreateTestFile(
            Path.Combine(_contentPath, "pages"),
            "test.md",
            "# Test\nThis is a test."
        );
        var defaultTemplate =
            "<html><head><title>Default Title</title></head><body>{{{Content}}}</body></html>";
        var templateProvider = new InMemoryTemplateProvider(
            new() { { "default", defaultTemplate } },
            []
        );

        await CreateConfig();

        var generator = new Generator(
            _contentPath,
            _outputPath,
            new(templateProvider),
            _configPath
        );

        // Act
        await generator.GenerateSiteAsync();

        // Assert
        var outputFile = Path.Combine(_outputPath, "pages", "test.html");
        Assert.True(File.Exists(outputFile));

        var content = await File.ReadAllTextAsync(outputFile);
        Assert.Contains("<title>Default Title</title>", content);
        Assert.Contains("<h1 id=\"test\">Test</h1>", content);
        Assert.Contains("<p>This is a test.</p>", content);
    }

    [Fact]
    public async Task GenerateSiteAsync_ShouldCreateBacklinks()
    {
        // Arrange
        await CreateTestFile(
            Path.Combine(_contentPath, "thoughts"),
            "note1.md",
            "This is [[note2]]."
        );
        await CreateTestFile(Path.Combine(_contentPath, "thoughts"), "note2.md", "This is note 2.");
        var noteTemplate =
            "<html><head><title>Note</title></head><body>{{{Content}}}</body></html>";
        var templateProvider = new InMemoryTemplateProvider(new() { { "note", noteTemplate } }, []);
        await CreateConfig();

        var generator = new Generator(
            _contentPath,
            _outputPath,
            new(templateProvider),
            _configPath
        );

        // Act
        await generator.GenerateSiteAsync();

        // Assert
        var note2OutputFile = Path.Combine(_outputPath, "thoughts", "note2.html");
        Assert.True(File.Exists(note2OutputFile));

        var content = await File.ReadAllTextAsync(note2OutputFile);
        Assert.Contains("<h2>Backlinks</h2>", content);
        Assert.Contains("<a href=\"note1.html\">note1</a>", content);
    }

    private static async Task CreateTestFile(string path, string fileName, string content)
    {
        var filePath = Path.Combine(path, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, content);
    }

    private async Task CreateConfig()
    {
        var config = new { SiteTitle = "Test Site", BaseUrl = "https://test.com" };
        await File.WriteAllTextAsync(_configPath, JsonSerializer.Serialize(config));
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
        return Task.CompletedTask;
    }
}
