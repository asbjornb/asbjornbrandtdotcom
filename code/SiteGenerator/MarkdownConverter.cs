using Markdig;

namespace SiteGenerator;

public class MarkdownConverter : IMarkdownConverter
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownConverter()
    {
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    public string ConvertToHtml(string markdown)
    {
        return Markdown.ToHtml(markdown, _pipeline);
    }
}
