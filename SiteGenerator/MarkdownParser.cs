using Markdig;

namespace SiteGenerator
{
    public class MarkdownParser
    {
        private readonly MarkdownPipeline _pipeline;

        public MarkdownParser()
        {
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        }

        public string ParseToHtml(string markdown)
        {
            return Markdown.ToHtml(markdown, _pipeline);
        }

        public async Task<string> ParseFileToHtmlAsync(string filePath)
        {
            try
            {
                string markdown = await File.ReadAllTextAsync(filePath);
                return ParseToHtml(markdown);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing file {filePath}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
