using Markdig;
using SiteGenerator.Templates;
using SiteGenerator.Templates.MetadataModels;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SiteGenerator.Processors;

public class PostProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;
    private readonly IFolderReader _folderReader;

    public PostProcessor(TemplateRenderer templateRenderer, IFolderReader folderReader)
    {
        _templateRenderer = templateRenderer;
        _folderReader = folderReader;
    }

    public async Task ProcessAsync(string inputPath, string outputPath)
    {
        await foreach (var contentFile in _folderReader.GetFileContents(inputPath, "*.md"))
        {
            var (frontMatter, markdownContent) = ExtractFrontMatter(contentFile.Content);

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

            var renderedContent = _templateRenderer.RenderPage(
                new LayoutModel(
                    frontMatter.ContainsKey("title")
                        ? frontMatter["title"].ToString()
                        : "SomeTitle",
                    frontMatter.ContainsKey("description")
                        ? frontMatter["description"].ToString()
                        : "SomeDescription",
                    "Website",
                    "SomeUrl",
                    htmlContent
                )
            );

            var fileName = Path.GetFileNameWithoutExtension(contentFile.Name);
            var outputFile = Path.Combine(outputPath, $"{fileName}.html");
            Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
            await File.WriteAllTextAsync(outputFile, renderedContent);
        }
    }

    private static (Dictionary<string, object> frontMatter, string content) ExtractFrontMatter(
        string fileContent
    )
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var frontMatter = new Dictionary<string, object>();
        var content = fileContent;

        if (fileContent.StartsWith("---"))
        {
            var end = fileContent.IndexOf("---", 3);
            if (end != -1)
            {
                var yaml = fileContent[3..end];
                frontMatter = deserializer.Deserialize<Dictionary<string, object>>(yaml);
                content = fileContent[(end + 3)..].Trim();
            }
        }

        return (frontMatter, content);
    }
}
