using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using SiteGenerator.Templates;
using Xunit;
using Xunit.Abstractions;

namespace SiteGenerator.Tests.Integration;

public sealed class GeneratorIntegrationTests : IAsyncLifetime, IDisposable
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

        var templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
        _generator = new Generator(InputPath, ActualOutputPath, templateRenderer, "config.json");
    }

    public async Task InitializeAsync()
    {
        // Generate all files once before any tests run
        await _generator.GenerateSiteAsync();
    }

    public Task DisposeAsync()
    {
        // Cleanup async resources if necessary
        return Task.CompletedTask;
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
    public void GenerateSite_CreatesAllExpectedFiles()
    {
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

    [Fact]
    public async Task GenerateCSharpNote_MatchesExpectedOutput()
    {
        // Compare normalized contents
        await CompareNormalizedContent(
            Path.Combine(ActualOutputPath, "notes", "csharp", "index.html"),
            Path.Combine(ExpectedOutputPath, "notes", "csharp", "index.html")
        );
    }

    [Fact]
    public async Task GenerateNowPage_MatchesExpectedOutput()
    {
        // Compare normalized contents
        await CompareNormalizedContent(
            Path.Combine(ActualOutputPath, "now", "index.html"),
            Path.Combine(ExpectedOutputPath, "now", "index.html")
        );
    }

    [Fact]
    public async Task GenerateInspirationPage_MatchesExpectedOutput()
    {
        // Compare normalized contents
        await CompareNormalizedContent(
            Path.Combine(ActualOutputPath, "inspiration", "index.html"),
            Path.Combine(ExpectedOutputPath, "inspiration", "index.html")
        );
    }

    [Fact]
    public async Task GenerateIndexPage_MatchesExpectedOutput()
    {
        // Compare normalized contents
        await CompareNormalizedContent(
            Path.Combine(ActualOutputPath, "index.html"),
            Path.Combine(ExpectedOutputPath, "index.html")
        );
    }

    private static async Task CompareNormalizedContent(
        string actualFilePath,
        string expectedFilePath
    )
    {
        var actualContent = await File.ReadAllTextAsync(actualFilePath);
        var expectedContent = await File.ReadAllTextAsync(expectedFilePath);

        var normalizedActual = NormalizeContent(actualContent).ToList();
        var normalizedExpected = NormalizeContent(expectedContent).ToList();

        int mismatchLine = -1;
        using (new AssertionScope())
        {
            for (var i = 0; i < Math.Min(normalizedActual.Count, normalizedExpected.Count); i++)
            {
                if (normalizedActual[i] != normalizedExpected[i])
                {
                    mismatchLine = i;
                    break;
                }
            }

            if (mismatchLine > -1)
            {
                normalizedActual[mismatchLine]
                    .Should()
                    .Be(
                        normalizedExpected[mismatchLine],
                        $"Mismatch at line {mismatchLine + 1}. Expected '{normalizedExpected[mismatchLine]}' but got '{normalizedActual[mismatchLine]}'."
                    );
            }

            normalizedActual
                .Should()
                .BeEquivalentTo(
                    normalizedExpected,
                    "The entire normalized content should match line by line."
                );
        }
    }

    private static IEnumerable<string> NormalizeContent(string content)
    {
        // Normalize line endings and remove empty lines
        content = content.Replace("\r\n", "\n");
        content = Regex.Replace(content, @"\n\s*</li>", "</li>");
        content = Regex.Replace(content, @"<li>\s*\n", "<li>");
        content = Regex.Replace(content, @"<li><p>(.*?)</p></li>", "<li>$1</li>");
        content = Regex.Replace(content, @"<ul>\s*\n", "<ul>");
        content = Regex.Replace(content, @"\n\s*</ul>", "</ul>");

        // Normalize image classes between old and new markdown parsing
        content = Regex.Replace(
            content,
            @"<p><img([^>]+)>\s*\{\.(\w+)\}</p>",
            "<p class=\"$2\"><img$1></p>"
        );

        // Normalize self-closing tags to standard closing
        content = Regex.Replace(content, @"\s*/>", ">");

        content = Regex.Replace(
            content,
            @"<a href=""(.*?)"">(.*?)</a>",
            match =>
            {
                string href = match.Groups[1].Value;
                string linkText = match.Groups[2].Value.Replace(" ", "-");
                return $"<a href=\"{href}\">{linkText}</a>";
            }
        );

        return Regex
            .Replace(content, @"<li><p>(.*?)</p></li>", "<li>$1</li>")
            .Replace("&#248;", "ø")
            .Replace("&#198;", "Æ")
            .Replace("&#230;", "æ")
            .Replace("&#216;", "Ø")
            .Replace("&#229;", "å")
            .Replace("&#197;", "Å")
            .Replace("&#8226;", "•")
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line));
    }
}
