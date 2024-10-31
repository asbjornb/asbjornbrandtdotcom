using FluentAssertions;
using SiteGenerator.Previews;
using Xunit;

namespace SiteGenerator.Tests;

public class PreviewGeneratorTests
{
    [Fact]
    public void GeneratePreview_ShouldReturnEmptyString_WhenInputIsEmpty()
    {
        // Arrange
        var htmlContent = string.Empty;

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GeneratePreview_ShouldReturnSameContent_WhenLengthIsLessThan200()
    {
        // Arrange
        var htmlContent = "This is a short text.";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        result.Should().Be(htmlContent);
    }

    [Fact]
    public void GeneratePreview_ShouldTruncatePlainText_WhenLengthExceeds200()
    {
        // Arrange
        var longText = new string('a', 250);

        // Act
        var result = PreviewGenerator.GeneratePreview(longText);

        // Assert
        var expected = new string('a', 200) + "...";
        result.Should().Be(expected);
    }

    [Fact]
    public void GeneratePreview_ShouldPreserveHtmlStructure_WhenContentIsLessThan200Chars()
    {
        // Arrange
        var htmlContent =
            "<p>This is a <strong>test</strong> of the <em>preview</em> generator.</p>";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        result.Should().Be(htmlContent);
    }

    [Fact]
    public void GeneratePreview_ShouldTruncateHtmlContentProperly_WhenLengthExceeds200()
    {
        // Arrange
        var htmlContent = "<p>" + new string('a', 250) + "</p>";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        var expected = "<p>" + new string('a', 200) + "...</p>";
        result.Should().Be(expected);
    }

    [Fact]
    public void GeneratePreview_ShouldHandleNestedHtmlElements_WhenTruncatingContent()
    {
        // Arrange
        var htmlContent =
            "<div><p>" + new string('a', 150) + "</p><p>" + new string('b', 100) + "</p></div>";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        var expected =
            "<div><p>" + new string('a', 150) + "</p><p>" + new string('b', 50) + "...</p></div>";
        result.Should().Be(expected);
    }

    [Fact]
    public void GeneratePreview_ShouldHandleHtmlEntitiesCorrectly()
    {
        // Arrange
        var htmlContent = "<p>&amp; &lt; &gt; &quot; &#39;</p>";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        result.Should().Be(htmlContent);
    }

    [Fact]
    public void GeneratePreview_ShouldNotCutOffInMiddleOfHtmlEntity()
    {
        // Arrange
        var htmlContent = "<p>" + new string('a', 198) + "&amp; more text</p>";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        var expected = "<p>" + new string('a', 198) + "&amp;...</p>";
        result.Should().Be(expected);
    }

    [Fact]
    public void GeneratePreview_ShouldHandleSpecialCharactersProperly()
    {
        // Arrange
        var htmlContent = "<p>Emoji: 😀😃😄😁😆😅😂🤣😊😇</p>";

        // Act
        var result = PreviewGenerator.GeneratePreview(htmlContent);

        // Assert
        result.Should().Be(htmlContent);
    }
}
