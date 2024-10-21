using Xunit;

namespace SiteGenerator.Tests;

public class MarkdownParserTests
{
    [Fact]
    public async Task ParseFileToHtmlAsync_ShouldReturnExpectedHtml()
    {
        // Arrange
        var parser = new MarkdownParser();
        var filePath = "example.md";
        var noteMapping = new Dictionary<string, string>();

        // Act
        string result = await parser.ParseFileToHtmlAsync(filePath, noteMapping);

        // Assert
        string expectedHtml =
            @"<h1 id=""example-markdown-file"">Example Markdown File</h1>
<p>This is an example Markdown file that will be parsed to HTML.</p>
<h2 id=""features"">Features</h2>
<ul>
<li>Lists</li>
<li><strong>Bold text</strong></li>
<li><em>Italic text</em></li>
<li><a href=""https://example.com"">Links</a></li>
</ul>
<blockquote>
<p>This is a blockquote.</p>
</blockquote>
";

        Assert.Equal(expectedHtml.Replace("\r\n", "\n"), result.Replace("\r\n", "\n"));
    }

    [Fact]
    public void ParseToHtml_ShouldReplaceObsidianLinks()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown = "This is a [[test]] of Obsidian-style links.";
        var noteMapping = new Dictionary<string, string> { { "test", "notes/test.html" } };

        // Act
        string result = parser.ParseToHtml(markdown, noteMapping);

        // Assert
        string expectedHtml =
            "<p>This is a <a href=\"notes/test.html\">test</a> of Obsidian-style links.</p>\n";
        Assert.Equal(expectedHtml, result);
    }

    [Fact]
    public void ParseToHtml_ShouldHandleOrdinaryMarkdownLinks()
    {
        // Arrange
        var parser = new MarkdownParser();
        var markdown =
            "This is a [regular link](https://example.com) and an [internal link](internal-page.md).";
        var noteMapping = new Dictionary<string, string>();

        // Act
        string result = parser.ParseToHtml(markdown, noteMapping);

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
        var noteMapping = new Dictionary<string, string>
        {
            { "obsidian-link", "notes/obsidian-link.html" }
        };

        // Act
        string result = parser.ParseToHtml(markdown, noteMapping);

        // Assert
        string expectedHtml =
            "<p>Here's an <a href=\"notes/obsidian-link.html\">obsidian-link</a> and a <a href=\"https://example.com\">regular link</a>.</p>\n";
        Assert.Equal(expectedHtml, result);
    }
}
