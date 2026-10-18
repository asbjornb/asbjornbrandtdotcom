using Markdig;
using SiteGenerator.MarkdownSupport;

namespace SiteGenerator;

public class MarkdownParser
{
    private readonly MarkdownPipeline _pipeline;
    private readonly PublishedNotes _publishedNotes;

    public MarkdownParser()
        : this(null, null) { }

    public MarkdownParser(PublishedNotes publishedNotes)
        : this(null, publishedNotes) { }

    public MarkdownParser(MarkdownPipeline? pipeline, PublishedNotes? publishedNotes = null)
    {
        _pipeline = pipeline ?? MarkdownPipelineFactory.GetPipeline();
        _publishedNotes = publishedNotes ?? PublishedNotes.All;
    }

    public string ParseToHtml(string markdown)
    {
        // Replace [[note-title]] with [note title](/notes/note-title/),
        // or plain "note title" when the note isn't published. Code blocks and spans are left alone.
        markdown = WikiLinks.Replace(
            markdown,
            match =>
            {
                var noteTitle = match.Groups[1].Value;
                var displayTitle = noteTitle.Replace("-", " ");
                return _publishedNotes.IsPublished(noteTitle)
                    ? $"[{displayTitle}](/notes/{noteTitle}/)"
                    : displayTitle;
            }
        );

        return Markdown.ToHtml(markdown, _pipeline);
    }
}
