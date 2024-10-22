using FluentAssertions;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;
using Xunit;

namespace SiteGenerator.Tests.TemplateTests;

public class TemplateRendererTests
{
    private readonly string _testTemplatePath = Path.Combine("TestData", "templates");

    [Fact]
    public void RenderTemplateWithoutBacklinks_ShouldRenderNoteTemplate()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var renderer = new TemplateRenderer(provider);

        var simpleNoteContent = "<h1>Simple Note</h1><p>This is a simple note.</p>";
        var noteData = new NoteModel(simpleNoteContent, []);
        var layoutData = new LayoutModel(
            "SomeTitle",
            "SomeDescription",
            "Website",
            "SomeUrl",
            null
        );

        // Act
        var renderedTemplate = renderer.RenderNote(noteData, layoutData);

        // Assert
        renderedTemplate.Should().NotBeNullOrEmpty();

        // Check for the presence of key HTML elements
        renderedTemplate.Should().Contain("<html");
        renderedTemplate.Should().Contain("<head>");
        renderedTemplate.Should().Contain("<title>SomeTitle</title>");
        renderedTemplate.Should().Contain("<body>");
        renderedTemplate.Should().Contain("<h1>Simple Note</h1>");
        renderedTemplate.Should().Contain("<p>This is a simple note.</p>");
        renderedTemplate.Should().Contain("Copyright Asbjørn Brandt");

        // Check that backlinks are not present
        renderedTemplate.Should().NotContain("<ul class=\"backlinks-container\">");
    }
}
