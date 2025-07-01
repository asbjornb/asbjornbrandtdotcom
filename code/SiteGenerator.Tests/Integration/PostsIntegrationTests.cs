using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using Xunit;
using Xunit.Abstractions;

namespace SiteGenerator.Tests.Integration;

public sealed class PostsIntegrationTests : IAsyncLifetime, IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly Generator _generator;
    private const string InputPath = "TestData/PostsTestInput";
    private const string ActualOutputPath = "TestOutput/Posts";

    public PostsIntegrationTests(ITestOutputHelper output)
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
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                Directory.Delete(path, true);
                return;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                Thread.Sleep(100 * (i + 1));
            }
            catch (UnauthorizedAccessException) when (i < maxRetries - 1)
            {
                Thread.Sleep(100 * (i + 1));
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
    public void PostsGeneration_CreatesExpectedPostFiles()
    {
        // Assert individual post files exist
        File.Exists(Path.Combine(ActualOutputPath, "posts", "test-post", "index.html"))
            .Should()
            .BeTrue();
        File.Exists(Path.Combine(ActualOutputPath, "posts", "first-post", "index.html"))
            .Should()
            .BeTrue();
        File.Exists(Path.Combine(ActualOutputPath, "posts", "second-post", "index.html"))
            .Should()
            .BeTrue();
    }

    [Fact]
    public void PostsGeneration_CreatesPostsIndexFile()
    {
        // Assert posts index exists
        File.Exists(Path.Combine(ActualOutputPath, "posts", "index.html")).Should().BeTrue();
    }

    [Fact]
    public async Task PostsIndex_ContainsAllPostsInCorrectOrder()
    {
        var indexContent = await File.ReadAllTextAsync(
            Path.Combine(ActualOutputPath, "posts", "index.html")
        );

        // Should contain all three posts
        indexContent.Should().Contain("Test Post Title");
        indexContent.Should().Contain("First Post");
        indexContent.Should().Contain("Second Post");

        // Should contain correct URLs
        indexContent.Should().Contain("href=\"https://asbjornbrandt.com/posts/test-post/\"");
        indexContent.Should().Contain("href=\"https://asbjornbrandt.com/posts/first-post/\"");
        indexContent.Should().Contain("href=\"https://asbjornbrandt.com/posts/second-post/\"");

        // Should be ordered by date (newest first)
        // Second Post (2025-01-20) should come before Test Post (2025-01-15) should come before First Post (2025-01-10)
        var secondPostIndex = indexContent.IndexOf("Second Post");
        var testPostIndex = indexContent.IndexOf("Test Post Title");
        var firstPostIndex = indexContent.IndexOf("First Post");

        secondPostIndex
            .Should()
            .BeLessThan(testPostIndex, "Second Post should appear before Test Post");
        testPostIndex
            .Should()
            .BeLessThan(firstPostIndex, "Test Post should appear before First Post");
    }

    [Fact]
    public async Task IndividualPost_ContainsExpectedContent()
    {
        var postContent = await File.ReadAllTextAsync(
            Path.Combine(ActualOutputPath, "posts", "test-post", "index.html")
        );

        // Should contain post title
        postContent.Should().Contain("<h1>Test Post Title</h1>");

        // Should contain formatted date
        postContent.Should().Contain("January 15, 2025");

        // Should contain post content without duplicate H1
        postContent.Should().Contain("This is a test post for integration testing");
        postContent.Should().Contain("<h2>A Subheading</h2>");
        postContent.Should().Contain("<strong>bold text</strong>");
        postContent.Should().Contain("<em>italic text</em>");

        // Should not have duplicate H1 tags
        var h1Count = Regex
            .Matches(postContent, @"<h1[^>]*>.*?</h1>", RegexOptions.IgnoreCase)
            .Count;
        h1Count
            .Should()
            .Be(1, "Should only have one H1 tag (the post header, not from markdown content)");
    }

    [Fact]
    public async Task PostsWithDifferentDates_AreOrderedCorrectly()
    {
        var indexContent = await File.ReadAllTextAsync(
            Path.Combine(ActualOutputPath, "posts", "index.html")
        );

        // Extract the order of dates in the content
        var dateMatches = Regex.Matches(indexContent, @"January (\d+), 2025");
        var dates = dateMatches.Cast<Match>().Select(m => int.Parse(m.Groups[1].Value)).ToList();

        // Should be in descending order (newest first)
        dates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task PostSlugGeneration_IsCorrect()
    {
        // Check that URLs are generated correctly from filenames
        var indexContent = await File.ReadAllTextAsync(
            Path.Combine(ActualOutputPath, "posts", "index.html")
        );

        // 2025-01-15-test-post.md -> test-post
        indexContent.Should().Contain("/posts/test-post/");

        // 2025-01-10-first-post.md -> first-post
        indexContent.Should().Contain("/posts/first-post/");

        // 2025-01-20-second-post.md -> second-post
        indexContent.Should().Contain("/posts/second-post/");
    }
}
