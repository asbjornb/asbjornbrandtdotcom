using Xunit;

namespace SiteGenerator.Tests;

public class MarkdownParserTests
{
    [Fact]
    public void ParseToHtml_ShouldParseBasicMarkdown()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = """
            # Example Markdown File

            This is an example Markdown file that will be parsed to HTML.

            ## Features

            * Lists
            * **Bold text**
            * *Italic text*
            * [Links](https://example.com)

            > This is a blockquote.
            """;

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml = """
            <h1 id="example-markdown-file">Example Markdown File</h1>
            <p>This is an example Markdown file that will be parsed to HTML.</p>
            <h2 id="features">Features</h2>
            <ul>
            <li>Lists</li>
            <li><strong>Bold text</strong></li>
            <li><em>Italic text</em></li>
            <li><a href="https://example.com">Links</a></li>
            </ul>
            <blockquote>
            <p>This is a blockquote.</p>
            </blockquote>
            """;

        // Normalize line endings and trim any trailing whitespace/newlines
        var normalizedExpected = expectedHtml.Replace("\r\n", "\n").Trim();
        var normalizedResult = result.Replace("\r\n", "\n").Trim();

        Assert.Equal(normalizedExpected, normalizedResult);
    }

    [Fact]
    public void ParseToHtml_ShouldReplaceObsidianLinks()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = "This is a [[test]] of Obsidian-style links.";

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml =
            "<p>This is a <a href=\"/test/\">test</a> of Obsidian-style links.</p>\n";
        Assert.Equal(expectedHtml, result);
    }

    [Fact]
    public void ParseToHtml_ShouldHandleOrdinaryMarkdownLinks()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown =
            "This is a [regular link](https://example.com) and an [internal link](internal-page.md).";

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml =
            "<p>This is a <a href=\"https://example.com\">regular link</a> and an <a href=\"internal-page.md\">internal link</a>.</p>\n";
        Assert.Equal(expectedHtml, result);
    }

    [Fact]
    public void ParseToHtml_ShouldHandleMixedLinkTypes()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = "Here's an [[obsidian-link]] and a [regular link](https://example.com).";

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml =
            "<p>Here's an <a href=\"/obsidian-link/\">obsidian link</a> and a <a href=\"https://example.com\">regular link</a>.</p>\n";
        Assert.Equal(expectedHtml, result);
    }
}
