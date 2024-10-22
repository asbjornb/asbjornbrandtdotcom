using FluentAssertions;
using HandlebarsDotNet;
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

        /// Use a multiline markdown string with a header and a single line
        var simpleNoteContent =
            @"# Simple Note
This is a simple note.";
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
    }
}
