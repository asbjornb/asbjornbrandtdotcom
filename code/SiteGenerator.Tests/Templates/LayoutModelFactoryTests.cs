using FluentAssertions;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using Xunit;

namespace SiteGenerator.Tests.Templates;

public class LayoutModelFactoryTests
{
    private readonly SiteMetadata _metadata =
        new(
            SiteTitle: "Test Site",
            BaseUrl: "https://example.com",
            Author: "Test Author",
            Description: "Test description"
        );

    [Fact]
    public void CreatePage_UsesSiteMetadataForTitleAndDescription()
    {
        var factory = new LayoutModelFactory(_metadata);

        var layout = factory.CreatePage("https://example.com/about/", "<p>content</p>");

        layout.MetaTitle.Should().Be("Test Site");
        layout.MetaDescription.Should().Be("Test description");
        layout.MetaType.Should().Be("website");
        layout.PageUrl.Should().Be("https://example.com/about/");
        layout.Body.Should().Be("<p>content</p>");
    }

    [Fact]
    public void CreateNote_ComposesAuthorBasedTitle()
    {
        var factory = new LayoutModelFactory(_metadata);

        var layout = factory.CreateNote("My Note", "https://example.com/notes/my-note/");

        layout.MetaTitle.Should().Be("My Note • Test Author's Notes");
        layout.MetaType.Should().Be("article");
        layout.Body.Should().BeNull();
    }

    [Fact]
    public void CreatePost_ComposesSiteTitle()
    {
        var factory = new LayoutModelFactory(_metadata);

        var layout = factory.CreatePost("Hello", "https://example.com/posts/hello/");

        layout.MetaTitle.Should().Be("Hello • Test Site");
        layout.MetaType.Should().Be("article");
    }

    [Fact]
    public void CreatePostsIndex_ComposesPostsTitle()
    {
        var factory = new LayoutModelFactory(_metadata);

        var layout = factory.CreatePostsIndex("https://example.com/posts/");

        layout.MetaTitle.Should().Be("Posts • Test Site");
        layout.MetaType.Should().Be("website");
    }
}
