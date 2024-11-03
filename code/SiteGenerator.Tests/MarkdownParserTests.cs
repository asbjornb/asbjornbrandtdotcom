using FluentAssertions;
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
            <h1>Example Markdown File</h1>
            <p>This is an example Markdown file that will be parsed to HTML.</p>
            <h2>Features</h2>
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

        var normalizedExpected = expectedHtml.Replace("\r\n", "\n").Trim();
        var normalizedResult = result.Replace("\r\n", "\n").Trim();

        normalizedResult.Should().Be(normalizedExpected);
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
            "<p>This is a <a href=\"/notes/test/\">test</a> of Obsidian-style links.</p>\n";
        result.Should().Be(expectedHtml);
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
        result.Should().Be(expectedHtml);
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
            "<p>Here's an <a href=\"/notes/obsidian-link/\">obsidian link</a> and a <a href=\"https://example.com\">regular link</a>.</p>\n";
        result.Should().Be(expectedHtml);
    }

    [Fact]
    public void ParseToHtml_ShouldReplaceHyphensWithSpacesInLinkTitles()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = "This is a link to [[note-title]] and another to [[another-note]].";

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml =
            "<p>This is a link to <a href=\"/notes/note-title/\">note title</a> and another to <a href=\"/notes/another-note/\">another note</a>.</p>\n";
        result.Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData("# Header 1", "<h1>Header 1</h1>\n")]
    [InlineData("## Header 2", "<h2>Header 2</h2>\n")]
    [InlineData("### Header 3", "<h3>Header 3</h3>\n")]
    public void ParseToHtml_ShouldConvertHeaders(string markdown, string expectedHtml)
    {
        // Arrange
        var parser = new MarkdownParser();

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        result.Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData("*italic*", "<p><em>italic</em></p>\n")]
    [InlineData("_italic_", "<p><em>italic</em></p>\n")]
    [InlineData("**bold**", "<p><strong>bold</strong></p>\n")]
    [InlineData("__bold__", "<p><strong>bold</strong></p>\n")]
    [InlineData("***bold italic***", "<p><em><strong>bold italic</strong></em></p>\n")]
    public void ParseToHtml_ShouldConvertEmphasis(string markdown, string expectedHtml)
    {
        // Arrange
        var parser = new MarkdownParser();

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        result.Should().Be(expectedHtml);
    }

    [Fact]
    public void ParseToHtml_ShouldConvertCodeBlocks()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = """
            ```csharp
            public class Example
            {
                public void Method() {}
            }
            ```
            """;

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml = """
            <pre><code class="language-csharp">public class Example
            {
                public void Method() {}
            }
            </code></pre>
            """;

        result.Trim().Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData("`code`", "<p><code>code</code></p>\n")]
    public void ParseToHtml_ShouldConvertInlineCode(string markdown, string expectedHtml)
    {
        // Arrange
        var parser = new MarkdownParser();

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        result.Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData(
        "[link text](https://example.com)",
        "<p><a href=\"https://example.com\">link text</a></p>\n"
    )]
    [InlineData("[link text](/local/path)", "<p><a href=\"/local/path\">link text</a></p>\n")]
    public void ParseToHtml_ShouldConvertLinks(string markdown, string expectedHtml)
    {
        // Arrange
        var parser = new MarkdownParser();

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        result.Should().Be(expectedHtml);
    }

    [Fact]
    public void ParseToHtml_ShouldConvertComplexMarkdown()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = """
            # Main Header

            This is a paragraph with **bold** and *italic* text.

            ## Subheader

            Here's some `inline code` and a [link](https://example.com).
            ```csharp
            public class Example {}
            ```
            """;

        // Act
        string result = parser.ParseToHtml(markdown);

        // Assert
        string expectedHtml = """
            <h1>Main Header</h1>
            <p>This is a paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
            <h2>Subheader</h2>
            <p>Here's some <code>inline code</code> and a <a href="https://example.com">link</a>.</p>
            <pre><code class="language-csharp">public class Example {}
            </code></pre>
            """;

        result.Trim().Should().Be(expectedHtml);
    }
}
