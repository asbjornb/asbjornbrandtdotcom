using System.Text.Json;
using Xunit;

namespace SiteGenerator.Tests;

public class GeneratorExplorationTests : IAsyncLifetime
{
    private string? _testRootPath;
    private string? _contentPath;
    private string? _outputPath;
    private string? _templatePath;
    private string? _configPath;

    public async Task InitializeAsync()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _contentPath = Path.Combine(_testRootPath, "TestContent");
        _outputPath = Path.Combine(_testRootPath, "TestOutput");
        _templatePath = Path.Combine(_testRootPath, "TestTemplates");
        _configPath = Path.Combine(_testRootPath, "config.json");

        Directory.CreateDirectory(_contentPath);
        Directory.CreateDirectory(Path.Combine(_contentPath, "pages"));
        Directory.CreateDirectory(Path.Combine(_contentPath, "posts"));
        Directory.CreateDirectory(Path.Combine(_contentPath, "notes"));
        Directory.CreateDirectory(_templatePath);

        await File.WriteAllTextAsync(
            Path.Combine(_contentPath, "pages", "test.md"),
            "# Test\nThis is a test."
        );
        await File.WriteAllTextAsync(
            Path.Combine(_templatePath, "default.html"),
            "<html><head><title>Default Title</title></head><body>{{{Content}}}</body></html>"
        );

        var config = new { SiteTitle = "Test Site", BaseUrl = "https://test.com" };
        await File.WriteAllTextAsync(_configPath, JsonSerializer.Serialize(config));
    }

    public Task DisposeAsync()
    {
        if (!string.IsNullOrEmpty(_testRootPath) && Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GenerateSiteAsync_ShouldCreateHtmlFiles()
    {
        // Arrange
        EnsureInitialized();
        var generator = new Generator(_contentPath!, _outputPath!, _templatePath!, _configPath!);

        // Act
        await generator.GenerateSiteAsync();

        // Assert
        var outputFile = Path.Combine(_outputPath!, "pages", "test.html");
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
        EnsureInitialized();
        await File.WriteAllTextAsync(
            Path.Combine(_contentPath!, "notes", "note1.md"),
            "This is [[note2]]."
        );
        await File.WriteAllTextAsync(
            Path.Combine(_contentPath!, "notes", "note2.md"),
            "This is note 2."
        );
        await File.WriteAllTextAsync(
            Path.Combine(_templatePath!, "note.html"),
            "<html><head><title>Note</title></head><body>{{{Content}}}</body></html>"
        );

        var generator = new Generator(_contentPath!, _outputPath!, _templatePath!, _configPath!);

        // Act
        await generator.GenerateSiteAsync();

        // Assert
        var note2OutputFile = Path.Combine(_outputPath!, "notes", "note2.html");
        Assert.True(File.Exists(note2OutputFile));

        var content = await File.ReadAllTextAsync(note2OutputFile);
        Assert.Contains("<h2>Backlinks</h2>", content);
        Assert.Contains("<a href=\"note1.html\">note1</a>", content);
    }

    private void EnsureInitialized()
    {
        if (
            _testRootPath == null
            || _contentPath == null
            || _outputPath == null
            || _templatePath == null
            || _configPath == null
        )
        {
            throw new InvalidOperationException(
                "Test environment not properly initialized. Ensure InitializeAsync has been called."
            );
        }
    }
}
