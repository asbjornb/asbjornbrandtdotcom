﻿using System.Text.RegularExpressions;
using Markdig;

namespace SiteGenerator;

public class MarkdownParser
{
    private readonly MarkdownPipeline _pipeline;

    public MarkdownParser()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAutoLinks()
            .UseEmphasisExtras()
            .UseTaskLists()
            .UseEmojiAndSmiley()
            .Build();
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
