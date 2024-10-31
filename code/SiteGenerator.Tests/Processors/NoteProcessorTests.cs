using NSubstitute;
using SiteGenerator.BacklinksProcessing;
using SiteGenerator.Processors;
using SiteGenerator.Templates;
using SiteGenerator.Tests.Helpers;
using Xunit;

namespace SiteGenerator.Tests.Processors;

public class NoteProcessorTests
{
    private readonly IFileProvider _fileProvider;
    private readonly MarkdownConverter _markdownConverter;
    private readonly TemplateRenderer _templateRenderer;
    private readonly Backlinks _backlinks;
    private readonly Config _config;

    private readonly NoteProcessor _processor;

    public NoteProcessorTests()
    {
        _fileProvider = Substitute.For<IFileProvider>();
        _markdownConverter = new MarkdownConverter();
        _templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
        _backlinks = new Backlinks();
        _config = new Config(
            SiteTitle: "Asbjørn Brandt",
            BaseUrl: "https://asbjornbrandt.com",
            Author: "Asbjørn Brandt",
            Description: "A small site mostly to play with static sites and share some thoughts and rough writing on tech, data and programming"
        );

        _processor = new NoteProcessor(
            _backlinks,
            _templateRenderer,
            _fileProvider,
            _markdownConverter,
            _config
        );
    }

    [Fact]
    public async Task ProcessAsync_ConvertsMarkdownAndRendersTemplate()
    {
        // Arrange
        var inputPath = "input";
        var outputPath = "output";
        var markdown = "# Test";

        _fileProvider
            .GetFileContents(inputPath, "*.md")
            .Returns(new[] { new ContentFile("test.md", markdown) }.ToAsyncEnumerable());

        // Act
        await _processor.ProcessAsync(inputPath, outputPath);

        // Assert
        await _fileProvider
            .Received(1)
            .WriteFileAsync(
                Arg.Is<string>(s => s == Path.Combine(outputPath, "test", "index.html")),
                Arg.Is<string>(s => s.Contains("<h1>Test</h1>"))
            );
    }

    [Fact]
    public async Task ProcessAsync_AddsBacklinksWhenPresent()
    {
        // Arrange
        var inputPath = "input";
        var outputPath = "output";
        var testMarkdown = "# Test";
        var otherNoteMarkdown = "# Other Note";
        var anotherNoteMarkdown = "# Another Note";

        _backlinks.AddBacklink("test", "other-note");
        _backlinks.AddBacklink("test", "another-note");

        _fileProvider
            .GetFileContents(inputPath, "*.md")
            .Returns(
                new[]
                {
                    new ContentFile("test.md", testMarkdown),
                    new ContentFile("other-note.md", otherNoteMarkdown),
                    new ContentFile("another-note.md", anotherNoteMarkdown)
                }.ToAsyncEnumerable()
            );

        // Act
        await _processor.ProcessAsync(inputPath, outputPath);

        // Assert
        await _fileProvider
            .Received(1)
            .WriteFileAsync(
                Arg.Is<string>(s => s == Path.Combine(outputPath, "test", "index.html")),
                Arg.Is<string>(s => s.Contains("/other-note/") && s.Contains("/another-note/"))
            );
    }

    [Fact]
    public async Task ProcessAsync_HandlesMultipleFiles()
    {
        // Arrange
        var inputPath = "input";
        var outputPath = "output";
        var files = new[]
        {
            new ContentFile("note1.md", "# Note 1"),
            new ContentFile("note2.md", "# Note 2")
        };

        _fileProvider.GetFileContents(inputPath, "*.md").Returns(files.ToAsyncEnumerable());

        // Act
        await _processor.ProcessAsync(inputPath, outputPath);

        // Assert
        await _fileProvider
            .Received(1)
            .WriteFileAsync(
                Arg.Is<string>(s => s == Path.Combine(outputPath, "note1", "index.html")),
                Arg.Any<string>()
            );
        await _fileProvider
            .Received(1)
            .WriteFileAsync(
                Arg.Is<string>(s => s == Path.Combine(outputPath, "note2", "index.html")),
                Arg.Any<string>()
            );
    }
}
