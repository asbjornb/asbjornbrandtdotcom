using Markdig;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Processors;

public class PageProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFileProvider _folderReader;

    public PageProcessor(TemplateRenderer templateRenderer, IFileProvider folderReader)
    {
        _templateRenderer = templateRenderer;
        _folderReader = folderReader;
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        await foreach (var contentFile in _folderReader.GetFileContents(inputPath, "*.md"))
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var htmlContent = Markdown.ToHtml(contentFile.Content, pipeline);

            var renderedContent = _templateRenderer.RenderPage(
                new LayoutModel("SomeTitle", "SomeDescription", "Website", "SomeUrl", htmlContent)
            );

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            if (fileName.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                // Put index.html directly in the output folder
                await _folderReader.WriteFileAsync(
                    Path.Combine(outputPath, "index.html"),
                    renderedContent
                );
            }
            else
            {
                // Create a folder for the page and put index.html inside it
                var pageFolder = Path.Combine(outputPath, fileName);
                await _folderReader.WriteFileAsync(
                    Path.Combine(pageFolder, "index.html"),
                    renderedContent
                );
            }
        }
    }
}
