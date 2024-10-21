using Markdig;

namespace SiteGenerator.Processors;

public class PageProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;

    public PageProcessor(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task ProcessAsync(string inputFile, string outputPath)
    {
        var content = await File.ReadAllTextAsync(inputFile);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlContent = Markdown.ToHtml(content, pipeline);

        var renderedContent = await _templateRenderer.RenderAsync(
            "default",
            new { Content = htmlContent }
        );

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        await File.WriteAllTextAsync(outputFile, renderedContent);
    }
}
