using FluentAssertions;
using Xunit;

namespace SiteGenerator.Tests;

public class MarkdownConverterTests
{
    private readonly MarkdownConverter _converter;

    public MarkdownConverterTests()
    {
        _converter = new MarkdownConverter();
    }

    [Theory]
    [InlineData("# Header 1", "<h1>Header 1</h1>")]
    [InlineData("## Header 2", "<h2>Header 2</h2>")]
    [InlineData("### Header 3", "<h3>Header 3</h3>")]
    public void ConvertsHeaders(string markdown, string expectedHtml)
    {
        // Act
        var result = _converter.ConvertToHtml(markdown);

        // Assert
        result.Trim().Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData("*italic*", "<p><em>italic</em></p>")]
    [InlineData("_italic_", "<p><em>italic</em></p>")]
    [InlineData("**bold**", "<p><strong>bold</strong></p>")]
    [InlineData("__bold__", "<p><strong>bold</strong></p>")]
    [InlineData("***bold italic***", "<p><em><strong>bold italic</strong></em></p>")]
    public void ConvertsEmphasis(string markdown, string expectedHtml)
    {
        // Act
        var result = _converter.ConvertToHtml(markdown);

        // Assert
        result.Trim().Should().Be(expectedHtml);
    }

    [Fact]
    public void ConvertsCodeBlocks()
    {
        // Arrange
        var markdown = """
            ```csharp
            public class Example
            {
                public void Method() {}
            }
            ```
            """;

        // Act
        var result = _converter.ConvertToHtml(markdown);

        // Assert
        result
            .Trim()
            .Should()
            .Be(
                """
                <pre><code class="language-csharp">public class Example
                {
                    public void Method() {}
                }
                </code></pre>
                """.Trim()
            );
    }

    [Theory]
    [InlineData("`code`", "<p><code>code</code></p>")]
    public void ConvertsInlineCode(string markdown, string expectedHtml)
    {
        // Act
        var result = _converter.ConvertToHtml(markdown);

        // Assert
        result.Trim().Should().Be(expectedHtml);
    }

    [Theory]
    [InlineData(
        "[link text](https://example.com)",
        "<p><a href=\"https://example.com\">link text</a></p>"
    )]
    [InlineData("[link text](/local/path)", "<p><a href=\"/local/path\">link text</a></p>")]
    public void ConvertsLinks(string markdown, string expectedHtml)
    {
        // Act
        var result = _converter.ConvertToHtml(markdown);

        // Assert
        result.Trim().Should().Be(expectedHtml);
    }

    [Fact]
    public void ConvertsComplexMarkdown()
    {
        // Arrange
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
        var result = _converter.ConvertToHtml(markdown);

        // Assert
        result
            .Trim()
            .Should()
            .Be(
                """
                <h1>Main Header</h1>
                <p>This is a paragraph with <strong>bold</strong> and <em>italic</em> text.</p>
                <h2>Subheader</h2>
                <p>Here's some <code>inline code</code> and a <a href="https://example.com">link</a>.</p>
                <pre><code class="language-csharp">public class Example {}
                </code></pre>
                """.Trim()
            );
    }
}
