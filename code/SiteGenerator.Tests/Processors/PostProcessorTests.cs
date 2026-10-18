using FluentAssertions;
using NSubstitute;
using SiteGenerator.Configuration;
using SiteGenerator.Processors;
using SiteGenerator.Templates;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests.Processors;

public class PostProcessorTests
{
    private readonly IFileProvider _mockFileProvider;
    private readonly TemplateRenderer _templateRenderer;
    private readonly MarkdownParser _markdownParser;
    private readonly SiteMetadata _siteMetadata;
    private readonly PostProcessor _postProcessor;

    public PostProcessorTests()
    {
        _mockFileProvider = Substitute.For<IFileProvider>();
        _templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
        _markdownParser = new MarkdownParser();
        _siteMetadata = new SiteMetadata(
            "Test Site",
            "https://test.com",
            "Test Author",
            "Test Description"
        );

        _postProcessor = new PostProcessor(
            _templateRenderer,
            _mockFileProvider,
            _markdownParser,
            _siteMetadata
        );
    }

    [Fact]
    public async Task ProcessAsync_WithValidPost_CreatesPostAndIndexFiles()
    {
        // Arrange
        var postContent = """
            # Test Post Title

            This is the content of the test post.
            """;

        var contentFiles = new[] { new ContentFile("2025-01-15-test-post.md", postContent) };

        _mockFileProvider
            .GetFileContents("input", "*.md")
            .Returns(contentFiles.ToAsyncEnumerable());

        // Act
        await _postProcessor.ProcessAsync("input", "output");

        // Assert
        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(
                Path.Combine("output", "posts", "test-post", "index.html"),
                Arg.Any<string>()
            );

        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(Path.Combine("output", "posts", "index.html"), Arg.Any<string>());
    }

    [Fact]
    public async Task ProcessAsync_WithMultiplePosts_CreatesAllPostFiles()
    {
        // Arrange
        var contentFiles = new[]
        {
            new ContentFile("2025-01-10-first-post.md", "# First Post\nContent"),
            new ContentFile("2025-01-20-second-post.md", "# Second Post\nContent"),
            new ContentFile("2025-01-15-third-post.md", "# Third Post\nContent"),
        };

        _mockFileProvider
            .GetFileContents("input", "*.md")
            .Returns(contentFiles.ToAsyncEnumerable());

        // Act
        await _postProcessor.ProcessAsync("input", "output");

        // Assert
        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(
                Path.Combine("output", "posts", "first-post", "index.html"),
                Arg.Any<string>()
            );

        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(
                Path.Combine("output", "posts", "second-post", "index.html"),
                Arg.Any<string>()
            );

        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(
                Path.Combine("output", "posts", "third-post", "index.html"),
                Arg.Any<string>()
            );

        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(Path.Combine("output", "posts", "index.html"), Arg.Any<string>());
    }

    [Theory]
    [InlineData("invalid-filename.md")]
    [InlineData("2025-13-01-invalid-month.md")]
    [InlineData("2025-01-32-invalid-day.md")]
    [InlineData("25-01-01-short-year.md")]
    [InlineData("2025-1-1-no-padding.md")]
    public async Task ProcessAsync_WithInvalidFileName_ThrowsArgumentException(
        string invalidFileName
    )
    {
        // Arrange
        var contentFiles = new[] { new ContentFile(invalidFileName, "# Test Title\nContent") };

        _mockFileProvider
            .GetFileContents("input", "*.md")
            .Returns(contentFiles.ToAsyncEnumerable());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postProcessor.ProcessAsync("input", "output")
        );

        exception.Message.Should().Contain(invalidFileName);
    }

    [Fact]
    public async Task ProcessAsync_WithNoPostsFiles_CreatesEmptyPostsIndex()
    {
        // Arrange
        _mockFileProvider
            .GetFileContents("input", "*.md")
            .Returns(Array.Empty<ContentFile>().ToAsyncEnumerable());

        // Act
        await _postProcessor.ProcessAsync("input", "output");

        // Assert - Should create posts index even with no posts
        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(Path.Combine("output", "posts", "index.html"), Arg.Any<string>());

        // Should not create any individual post files
        await _mockFileProvider
            .DidNotReceive()
            .WriteFileAsync(
                Arg.Is<string>(path =>
                    path.Contains("posts")
                    && path.Contains("index.html")
                    && !path.EndsWith(Path.Combine("posts", "index.html"))
                ),
                Arg.Any<string>()
            );
    }

    [Theory]
    [InlineData("2025-01-15-test-post.md", "test-post")]
    [InlineData("2024-12-31-new-year-eve.md", "new-year-eve")]
    [InlineData("2025-02-14-valentine-special.md", "valentine-special")]
    [InlineData("2025-03-01-march-first.md", "march-first")]
    public async Task ProcessAsync_WithDifferentFileNames_CreatesCorrectSlugs(
        string fileName,
        string expectedSlug
    )
    {
        // Arrange
        var contentFiles = new[] { new ContentFile(fileName, "# Test Title\nContent") };

        _mockFileProvider
            .GetFileContents("input", "*.md")
            .Returns(contentFiles.ToAsyncEnumerable());

        // Act
        await _postProcessor.ProcessAsync("input", "output");

        // Assert
        await _mockFileProvider
            .Received(1)
            .WriteFileAsync(
                Path.Combine("output", "posts", expectedSlug, "index.html"),
                Arg.Any<string>()
            );
    }
}
