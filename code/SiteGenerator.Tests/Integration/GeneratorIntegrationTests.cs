using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
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

        var templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
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

    [Fact]
    public async Task GenerateNote_MatchesExpectedOutput()
    {
        // Act
        await _generator.GenerateSiteAsync();

        // Read files
        var actualContent = await File.ReadAllTextAsync(
            Path.Combine(ActualOutputPath, "notes", "csharp", "index.html")
        );
        var expectedContent = await File.ReadAllTextAsync(
            Path.Combine(ExpectedOutputPath, "notes", "csharp", "index.html")
        );

        static IEnumerable<string> NormalizeContent(string content)
        {
            // Normalize list items by removing <p> tags within <li>, as Markdig adds <p> for best-practice formatting.
            // This keeps test comparisons focused on content rather than HTML structural differences.
            content = content.Replace("\r\n", "\n"); // Standardize newline format
            content = Regex.Replace(content, @"\n\s*</li>", "</li>"); // Remove newlines before <li>
            content = Regex.Replace(content, @"<li>\s*\n", "<li>"); // Remove newlines after </li>
            content = Regex.Replace(content, @"<li><p>(.*?)</p></li>", "<li>$1</li>"); // Remove <p> tags within <li>
            // Remove newlines before and after <ul>
            content = Regex.Replace(content, @"<ul>\s*\n", "<ul>");
            content = Regex.Replace(content, @"\n\s*</ul>", "</ul>");

            // Add normalization for link text with hyphen vs. space differences
            content = Regex.Replace(
                content,
                @"<a href=""(.*?)"">(.*?)</a>",
                match =>
                {
                    string href = match.Groups[1].Value;
                    string linkText = match.Groups[2].Value.Replace(" ", "-"); // Replace space with hyphen in link text
                    return $"<a href=\"{href}\">{linkText}</a>";
                }
            );

            return Regex
                .Replace(content, @"<li><p>(.*?)</p></li>", "<li>$1</li>") // Remove <p> tags within <li>
                .Replace("&#248;", "ø") // Replace specific HTML entities with equivalent characters
                .Replace("&#198;", "Æ")
                .Replace("&#230;", "æ")
                .Replace("&#216;", "Ø")
                .Replace("&#229;", "å")
                .Replace("&#197;", "Å")
                .Replace("&#8226;", "•") // Replace bullet character entity
                .Split('\n')
                .Select(line => line.Trim()) // Trim whitespace from each line for cleaner comparisons
                .Where(line => !string.IsNullOrEmpty(line)); // Ignore blank lines for focused comparison
        }

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
                    break; // Break the loop on the first mismatch
                }
            }

            // Report the first error found
            if (mismatchLine > -1)
            {
                normalizedActual[mismatchLine]
                    .Should()
                    .Be(
                        normalizedExpected[mismatchLine],
                        $"Mismatch at line {mismatchLine + 1}. Expected '{normalizedExpected[mismatchLine]}' but got '{normalizedActual[mismatchLine]}'."
                    );
            }

            // Final assertion to check if the entire content matches
            normalizedActual
                .Should()
                .BeEquivalentTo(
                    normalizedExpected,
                    "The entire normalized content should match line by line."
                );
        }
    }
}
