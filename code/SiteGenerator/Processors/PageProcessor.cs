using Markdig;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class PageProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;

    public PageProcessor(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task ProcessAsync(IFolderReader folderReader, string inputPath, string outputPath)
    {
        await foreach (var contentFile in folderReader.GetFileContents(inputPath, "*.md"))
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var htmlContent = Markdown.ToHtml(contentFile.Content, pipeline);

            var renderedContent = _templateRenderer.RenderPage(
                new LayoutModel("SomeTitle", "SomeDescription", "Website", "SomeUrl", htmlContent)
            );

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var outputFile = Path.Combine(outputPath, $"{fileName}.html");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
            await File.WriteAllTextAsync(outputFile, renderedContent);
        }
    }
}
