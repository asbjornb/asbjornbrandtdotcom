using FluentAssertions;
using Markdig;
using SiteGenerator.MarkdownSupport;
using Xunit;

namespace SiteGenerator.Tests.Markdown;

public class MarkdownPipelineFactoryTests
{
    [Fact]
    public void GetPipeline_ReturnsCachedInstance()
    {
        var first = MarkdownPipelineFactory.GetPipeline();
        var second = MarkdownPipelineFactory.GetPipeline();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void MarkdownParser_DefaultCtor_UsesFactoryPipeline()
    {
        var pipeline = MarkdownPipelineFactory.GetPipeline();
        var parser = new MarkdownParser();

        // The parser should generate identical output for wiki links, verifying configuration is shared.
        var sample = "Check [[note-title]]";

        var result = parser.ParseToHtml(sample);

        result.Should().Contain("/notes/note-title/");
    }

    [Fact]
    public void MarkdownParser_CanAcceptCustomPipeline()
    {
        var pipeline = new MarkdownPipelineBuilder().Build();
        var parser = new MarkdownParser(pipeline);

        parser.ParseToHtml("text").Should().Be("<p>text</p>\n");
    }
}
