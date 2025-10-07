using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using Xunit;
using Xunit.Abstractions;

namespace SiteGenerator.Tests.Integration;

public sealed class GeneratorIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Generator _generator;
    private const string InputPath = "TestData/OldSiteInput";
    private const string ActualOutputPath = "TestOutput_GeneratorIntegration";

    public GeneratorIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory(ActualOutputPath);
        var siteMetadata =
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("SiteMetadata")
                .Get<SiteMetadata>()
            ?? throw new Exception("Could not bind configuration sections to records.");

        var templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
        _generator = new Generator(InputPath, ActualOutputPath, templateRenderer, siteMetadata);
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
            DeleteDirectoryWithRetry(ActualOutputPath);
        }
    }

    private static void DeleteDirectoryWithRetry(string path, int maxRetries = 3)
    {
        if (!Directory.Exists(path))
            return;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                Directory.Delete(path, true);
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                // Wait a bit for any file handles to be released
                Thread.Sleep(100 * (i + 1));
            }
            catch (UnauthorizedAccessException) when (i < maxRetries - 1)
            {
                // Wait a bit for any file handles to be released
                Thread.Sleep(100 * (i + 1));
            }
            catch (DirectoryNotFoundException)
            {
                // Directory already doesn't exist, consider this success
                return;
            }
        }

        // If we still can't delete after retries, try to delete individual files
        try
        {
            DeleteDirectoryContents(path);
            Directory.Delete(path, false);
        }
        catch
        {
            // If cleanup still fails, ignore it - it's just test cleanup
            // The test results are still valid
        }
    }

    private static void DeleteDirectoryContents(string path)
    {
        if (!Directory.Exists(path))
            return;

        foreach (var file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            try
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
            catch
            {
                // Ignore individual file deletion failures
            }
        }

        foreach (var dir in Directory.GetDirectories(path))
        {
            try
            {
                DeleteDirectoryContents(dir);
                Directory.Delete(dir, false);
            }
            catch
            {
                // Ignore individual directory deletion failures
            }
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
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // Allow up to 30 seconds in CI
    }

    [Fact]
    public void GenerateSite_CreatesAllExpectedHtmlFiles()
    {
        var expectedFiles = GetExpectedHtmlRelativePaths();

        var actualFiles = Directory
            .GetFiles(ActualOutputPath, "*.html", SearchOption.AllDirectories)
            .Select(path => NormalizeRelativePath(ActualOutputPath, path))
            .OrderBy(f => f)
            .ToList();

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
    public void GenerateSite_CopiesAllExpectedAssets()
    {
        var expectedFiles = GetExpectedAssetRelativePaths();

        var actualFiles = Directory
            .GetFiles(Path.Combine(ActualOutputPath, "assets"), "*", SearchOption.AllDirectories)
            .Select(path =>
                "assets/" + NormalizeRelativePath(Path.Combine(ActualOutputPath, "assets"), path)
            )
            .OrderBy(f => f)
            .ToList();

        actualFiles.Should().BeEquivalentTo(expectedFiles);
    }

    [Fact]
    public void GenerateCSharpNote_ShouldRenderExpectedContent()
    {
        var content = ReadOutputFile("notes", "csharp", "index.html");

        content.Should().Contain("<h1>C#</h1>");
        content.Should().Contain("Single responsibility principle");
        content.Should().Contain("global-graph-modal");
        content.Should().Contain("Backlinks");
    }

    [Fact]
    public void GeneratedNotesWithConnections_ShouldIncludeGlobalGraphModal()
    {
        // Arrange
        var noteWithConnectionsPath = Path.Combine(
            ActualOutputPath,
            "notes",
            "csharp",
            "index.html"
        );

        // Act
        var content = File.ReadAllText(noteWithConnectionsPath);

        // Assert
        content
            .Should()
            .Contain(
                "global-graph-modal",
                "notes with connections should include the global graph modal"
            );
        content
            .Should()
            .Contain("openGlobalGraph()", "should have the function to open global graph");
        content
            .Should()
            .Contain("knowledge-graph.js", "should include the knowledge graph JavaScript");
        content.Should().Contain("zoom-btn", "should include zoom controls in the modal");
        content.Should().Contain("legend", "should include the legend in the modal");
    }

    [Fact]
    public void GraphDataFile_ShouldBeGenerated()
    {
        // Arrange
        var graphDataPath = Path.Combine(ActualOutputPath, "assets", "graph-data.json");

        // Act & Assert
        File.Exists(graphDataPath).Should().BeTrue("graph-data.json should be generated");

        var content = File.ReadAllText(graphDataPath);
        content.Should().NotBeEmpty("graph data should not be empty");
        content.Should().Contain("nodes", "should contain nodes array");
        content.Should().Contain("links", "should contain links array");
        content.Should().Contain("stats", "should contain statistics");
    }

    [Fact]
    public void GenerateNowPage_ShouldIncludeHeadingsAndLinks()
    {
        var content = ReadOutputFile("now", "index.html");

        content.Should().Contain("<h1>Now</h1>");
        content.Should().Contain("Recently had a 2 months playthrough");
        content.Should().Contain("href=\"https://www.youtube.com/watch?v=YicXdyDFWuw\"");
    }

    [Fact]
    public void GenerateInspirationPage_ShouldIncludeQuotes()
    {
        var content = ReadOutputFile("inspiration", "index.html");

        content.Should().Contain("Inspiration for this site");
        content.Should().Contain("A digital garden");
        content.Should().Contain("Maggie Appleton");
    }

    [Fact]
    public void GenerateIndexPage_ShouldExposeNavigation()
    {
        var content = ReadOutputFile("index.html");

        content.Should().Contain("<a href=\"/notes\">Notes</a>");
        content.Should().Contain("<a href=\"/posts\">Posts</a>");
        content.Should().Contain("A small site mostly to play with static sites");
    }

    private static string ReadOutputFile(params string[] relativeSegments)
    {
        var path = BuildPath(ActualOutputPath, relativeSegments);
        return File.ReadAllText(path);
    }

    private static string BuildPath(string root, params string[] segments)
    {
        var path = root;
        foreach (var segment in segments)
        {
            path = Path.Combine(path, segment);
        }
        return path;
    }

    private static string NormalizeRelativePath(string root, string fullPath)
    {
        var relative = fullPath[root.Length..]
            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return relative.Replace('\\', '/');
    }

    private static List<string> GetExpectedHtmlRelativePaths()
    {
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.GetFiles(Path.Combine(InputPath, "thoughts"), "*.md"))
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var relative = slug.Equals("index", StringComparison.OrdinalIgnoreCase)
                ? "notes/index.html"
                : $"notes/{slug}/index.html";
            expected.Add(relative);
        }

        foreach (var file in Directory.GetFiles(Path.Combine(InputPath, "pages"), "*.md"))
        {
            var slug = Path.GetFileNameWithoutExtension(file);
            var relative = slug.Equals("index", StringComparison.OrdinalIgnoreCase)
                ? "index.html"
                : $"{slug}/index.html";
            expected.Add(relative);
        }

        foreach (var file in Directory.GetFiles(Path.Combine(InputPath, "posts"), "*.md"))
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var match = Regex.Match(fileName, @"^(\d{4}-\d{2}-\d{2})-(.+)$");
            if (!match.Success)
            {
                continue;
            }

            var slug = match.Groups[2].Value;
            expected.Add($"posts/{slug}/index.html");
        }

        expected.Add("posts/index.html");

        return expected.Select(e => e.Replace('\\', '/')).OrderBy(e => e).ToList();
    }

    private static List<string> GetExpectedAssetRelativePaths()
    {
        var assetRoot = Path.Combine(InputPath, "assets");

        if (!Directory.Exists(assetRoot))
        {
            return new List<string>();
        }

        var assets = Directory
            .GetFiles(assetRoot, "*", SearchOption.AllDirectories)
            .Select(path => "assets/" + NormalizeRelativePath(assetRoot, path))
            .ToList();

        assets.Add("assets/graph-data.json");

        return assets.OrderBy(f => f).ToList();
    }
}
