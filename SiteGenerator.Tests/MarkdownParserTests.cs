using Xunit;
using SiteGenerator;
using System.Threading.Tasks;

namespace SiteGenerator.Tests
{
    public class MarkdownParserTests
    {
        [Fact]
        public async Task ParseFileToHtmlAsync_ShouldReturnExpectedHtml()
        {
            // Arrange
            var parser = new MarkdownParser();
            var filePath = "example.md";

            // Act
            string result = await parser.ParseFileToHtmlAsync(filePath);

            // Assert
            string expectedHtml = @"<h1 id=""example-markdown-file"">Example Markdown File</h1>
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
    }
}
