using FluentAssertions;
using HandlebarsDotNet;
using SiteGenerator.Templates;
using Xunit;

namespace SiteGenerator.Tests.TemplateTests;

public class FileTemplateProviderTests
{
    private readonly string _testTemplatePath = Path.Combine("TestData", "templates");

    [Fact]
    public void GetTemplateContent_ShouldReadNoteTemplate()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var templateName = "note";

        // Act
        var templateContent = provider.GetTemplateContent(templateName);

        // Assert
        templateContent.Should().NotBeNullOrEmpty();
        var compiledTemplate = Handlebars.Compile(templateContent);
        compiledTemplate.Should().NotBeNull();
    }

    [Fact]
    public void GetTemplateContent_ShouldReadLayoutTemplate()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var templateName = "layout";

        // Act
        var templateContent = provider.GetTemplateContent(templateName);

        // Assert
        templateContent.Should().NotBeNullOrEmpty();
        var compiledTemplate = Handlebars.Compile(templateContent);
        compiledTemplate.Should().NotBeNull();
    }

    [Fact]
    public void GetPartialContent_ShouldReadHeadPartial()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var partialName = "head";

        // Act
        var partialContent = provider.GetPartialContent(partialName);

        // Assert
        partialContent.Should().NotBeNullOrEmpty();
        Handlebars.RegisterTemplate(partialName, partialContent);
        var template = Handlebars.Compile("{{> head}}");
        template.Should().NotBeNull();
    }

    [Fact]
    public void GetPartialContent_ShouldReadBacklinksPartial()
    {
        // Arrange
        var provider = new FileTemplateProvider(_testTemplatePath);
        var partialName = "backlinks";

        // Act
        var partialContent = provider.GetPartialContent(partialName);

        // Assert
        partialContent.Should().NotBeNullOrEmpty();
        Handlebars.RegisterTemplate(partialName, partialContent);
        var template = Handlebars.Compile("{{> backlinks}}");
        template.Should().NotBeNull();
    }
}
