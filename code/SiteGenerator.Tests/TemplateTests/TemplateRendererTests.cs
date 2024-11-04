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

    [Fact]
    public void RenderTemplateWithBacklinks_ShouldRenderNoteTemplateWithBacklinks()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var renderer = new TemplateRenderer(provider);

        var noteContent = @"<h1>Note with Backlinks</h1>";
        var backlinks = new List<BacklinkModel>
        {
            new(Url: "/note1.html", Title: "Note 1", PreviewHtml: "<p>Preview of Note 1</p>"),
            new(Url: "/note2.html", Title: "Note 2", PreviewHtml: "<p>Preview of Note 2</p>"),
        };

        var noteData = new NoteModel(noteContent, backlinks);
        var layoutData = new LayoutModel(
            MetaTitle: "Note with Backlinks",
            MetaDescription: "SomeDescription",
            MetaType: "Website",
            PageUrl: "SomeUrl",
            Body: null
        );

        // Act
        var renderedTemplate = renderer.RenderNote(noteData, layoutData);

        // Assert
        renderedTemplate.Should().NotBeNullOrEmpty();
        renderedTemplate.Should().Contain("<h1>Note with Backlinks</h1>");

        // Check that backlinks are present
        renderedTemplate.Should().Contain("<ul class=\"backlinks-container\">");
        renderedTemplate
            .Should()
            .Contain("<a href=\"/note1.html\" class=\"backlink__link\">Note 1</a>");
        renderedTemplate.Should().Contain("<p>Preview of Note 1</p>");
        renderedTemplate
            .Should()
            .Contain("<a href=\"/note2.html\" class=\"backlink__link\">Note 2</a>");
        renderedTemplate.Should().Contain("<p>Preview of Note 2</p>");
    }

    [Fact]
    public void RenderPageTemplate_ShouldRender()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var renderer = new TemplateRenderer(provider);

        var simplePageContent = "<h1>Simple Page</h1><p>Some words</p>";
        var layoutData = new LayoutModel(
            "SomeTitle",
            "SomeDescription",
            "Website",
            "SomeUrl",
            simplePageContent
        );

        // Act
        var renderedTemplate = renderer.RenderPage(layoutData);

        // Assert
        renderedTemplate.Should().NotBeNullOrEmpty();

        // Check for the presence of key HTML elements
        renderedTemplate.Should().Contain("<html");
        renderedTemplate.Should().Contain("<head>");
        renderedTemplate.Should().Contain("<title>SomeTitle</title>");
        renderedTemplate.Should().Contain("<body>");
        renderedTemplate.Should().Contain("<h1>Simple Page</h1>");
        renderedTemplate.Should().Contain("<p>Some words</p>");
        renderedTemplate.Should().Contain("Copyright Asbjørn Brandt");
    }
}
