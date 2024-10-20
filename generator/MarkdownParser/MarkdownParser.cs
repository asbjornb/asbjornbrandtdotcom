using Markdig;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class MarkdownParser
{
    public static string ConvertMarkdownToHtml(string markdownContent, Dictionary<string, string> noteMapping)
    {
        markdownContent = ReplaceObsidianLinks(markdownContent, noteMapping);

        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        return Markdown.ToHtml(markdownContent, pipeline);
    }

    private static string ReplaceObsidianLinks(string content, Dictionary<string, string> noteMapping)
    {
        return Regex.Replace(content, @"\[\[(.*?)\]\]", match =>
        {
            var noteName = match.Groups[1].Value;
            if (noteMapping.TryGetValue(noteName, out var noteUrl))
            {
                return $"[{noteName}]({noteUrl})";
            }
            return match.Value; // Keep original if not found in mapping
        });
    }
}
