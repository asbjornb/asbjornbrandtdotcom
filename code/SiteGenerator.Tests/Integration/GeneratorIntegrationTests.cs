using System.Diagnostics;
using FluentAssertions;
using SiteGenerator.Templates;
using Xunit;
using Xunit.Abstractions;

namespace SiteGenerator.Tests.Integration;

public sealed class GeneratorIntegrationTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Generator _generator;
    private const string InputPath = "TestData/OldSiteInput";
    private const string ExpectedOutputPath = "TestData/OldSiteOutput";
    private const string ActualOutputPath = "TestOutput";

    public GeneratorIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(ActualOutputPath);

        var templateRenderer = new TemplateRenderer(new FileTemplateProvider("testdata/templates"));
        _generator = new Generator(InputPath, ActualOutputPath, templateRenderer, "config.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(ActualOutputPath))
        {
            Directory.Delete(ActualOutputPath, true);
        }
    }

    [Fact]
    public async Task GenerateSite_CompletesWithinReasonableTime()
    {
        // Arrange
        var stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        await _generator.GenerateSiteAsync();
        stopwatch.Stop();

        // Assert
        _output.WriteLine($"Site generation took: {stopwatch.ElapsedMilliseconds}ms");
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // 5 seconds should be plenty
    }

    [Fact]
    public async Task GenerateSite_CreatesAllExpectedFiles()
    {
        // Act
        await _generator.GenerateSiteAsync();

        // Assert
        var expectedFiles = Directory
            .GetFiles(ExpectedOutputPath, "*.html", SearchOption.AllDirectories)
            .Select(f => f.Replace(ExpectedOutputPath, "").TrimStart('\\'))
            .OrderBy(f => f)
            .ToList();

        var actualFiles = Directory
            .GetFiles(ActualOutputPath, "*.html", SearchOption.AllDirectories)
            .Select(f => f.Replace(ActualOutputPath, "").TrimStart('\\'))
            .OrderBy(f => f)
            .ToList();

        // Log the differences
        _output.WriteLine("Files in actual but not in expected:");
        foreach (var file in actualFiles.Except(expectedFiles))
        {
            _output.WriteLine($"  {file}");
        }

        _output.WriteLine("\nFiles in expected but not in actual:");
        foreach (var file in expectedFiles.Except(actualFiles))
        {
            _output.WriteLine($"  {file}");
        }

        actualFiles.Should().BeEquivalentTo(expectedFiles);
    }
}
