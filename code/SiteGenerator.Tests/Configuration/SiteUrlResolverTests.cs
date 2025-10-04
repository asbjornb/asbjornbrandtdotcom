using FluentAssertions;
using SiteGenerator.Configuration;
using Xunit;

namespace SiteGenerator.Tests.Configuration;

public class SiteUrlResolverTests
{
    private readonly SiteMetadata _metadata =
        new(
            SiteTitle: "Test Site",
            BaseUrl: "https://example.com/",
            Author: "Author",
            Description: "Description"
        );

    [Fact]
    public void Note_ReturnsNotesUrlWithTrailingSlash()
    {
        var resolver = new SiteUrlResolver(_metadata);

        resolver.Note("my-note").Should().Be("https://example.com/notes/my-note/");
    }

    [Theory]
    [InlineData("index", "https://example.com")]
    [InlineData("about", "https://example.com/about/")]
    public void Page_ReturnsExpectedUrl(string slug, string expected)
    {
        var resolver = new SiteUrlResolver(_metadata);

        resolver.Page(slug).Should().Be(expected);
    }

    [Fact]
    public void Post_ReturnsPostUrlWithTrailingSlash()
    {
        var resolver = new SiteUrlResolver(_metadata);

        resolver.Post("hello-world").Should().Be("https://example.com/posts/hello-world/");
    }

    [Fact]
    public void PostsIndex_ReturnsPostsIndexUrl()
    {
        var resolver = new SiteUrlResolver(_metadata);

        resolver.PostsIndex().Should().Be("https://example.com/posts/");
    }
}
