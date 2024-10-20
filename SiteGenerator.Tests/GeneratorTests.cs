using Xunit;

namespace SiteGenerator.Tests;

public class SiteGeneratorTests : IAsyncLifetime
{
    private string _testRootPath;
    private string _contentPath;
    private string _outputPath;
    private string _templatePath;

    public async Task InitializeAsync()
    {
        _testRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _contentPath = Path.Combine(_testRootPath, "TestContent");
        _outputPath = Path.Combine(_testRootPath, "TestOutput");
        _templatePath = Path.Combine(_testRootPath, "TestTemplates");

        Directory.CreateDirectory(_contentPath);
        Directory.CreateDirectory(_templatePath);
        await File.WriteAllTextAsync(
            Path.Combine(_contentPath, "test.md"),
            "# Test\nThis is a test."
        );
        await File.WriteAllTextAsync(
            Path.Combine(_templatePath, "default.html"),
            "<html><body>{{{Content}}}</body></html>"
        );
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_testRootPath))
        {
            Directory.Delete(_testRootPath, true);
        }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GenerateSiteAsync_ShouldCreateHtmlFiles()
    {
        // Arrange
        var generator = new Generator(_contentPath, _outputPath, _templatePath);

        // Act
        await generator.GenerateSiteAsync();

        // Assert
        var outputFile = Path.Combine(_outputPath, "test.html");
        Assert.True(File.Exists(outputFile));

        var content = await File.ReadAllTextAsync(outputFile);
        Assert.Contains("<html><body><h1 id=\"test\">Test</h1>", content);
        Assert.Contains("<p>This is a test.</p></body></html>", content);
    }
}
