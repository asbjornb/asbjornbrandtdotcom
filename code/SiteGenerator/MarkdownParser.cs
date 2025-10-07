using System.Text.RegularExpressions;
using Markdig;
using SiteGenerator.MarkdownSupport;

namespace SiteGenerator;

public class MarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownParser()
        : this(null) { }

    public MarkdownParser(MarkdownPipeline? pipeline)
    {
        _pipeline = pipeline ?? MarkdownPipelineFactory.GetPipeline();
    }

    public string ParseToHtml(string markdown)
    {
        // Replace [[note-title]] with [note title](/note-title/)
        markdown = Regex.Replace(
            markdown,
            @"\[\[(.+?)\]\]",
            match =>
            {
                var noteTitle = match.Groups[1].Value;
                var displayTitle = noteTitle.Replace("-", " ");
                return $"[{displayTitle}](/notes/{noteTitle}/)";
            }
        );

        return Markdown.ToHtml(markdown, _pipeline);
    }
}
