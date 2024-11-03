using System.Text.RegularExpressions;
using Markdig;

namespace SiteGenerator;

public class MarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownParser()
    {
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    public string ParseToHtml(string markdown, Dictionary<string, string> noteMapping)
    {
        // Replace [[note-title]] with [note-title](note-title.html)
        foreach (var note in noteMapping)
        {
            markdown = Regex.Replace(
                markdown,
                $@"\[\[{Regex.Escape(note.Key)}\]\]",
                $"[{note.Key}]({note.Value})"
            );
        }

        return Markdown.ToHtml(markdown, _pipeline);
    }
}
