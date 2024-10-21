using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SiteGenerator;

public class PostProcessor : IPageProcessor
{
    private readonly TemplateRenderer _templateRenderer;

    public PostProcessor(TemplateRenderer templateRenderer)
    {
        _templateRenderer = templateRenderer;
    }

    public async Task ProcessAsync(string inputFile, string outputPath)
    {
        var content = await File.ReadAllTextAsync(inputFile);
        var (frontMatter, markdownContent) = ExtractFrontMatter(content);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

        var renderedContent = await _templateRenderer.RenderAsync(
            "post",
            new { Metadata = frontMatter, Content = htmlContent }
        );

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        var outputFile = Path.Combine(outputPath, $"{fileName}.html");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        await File.WriteAllTextAsync(outputFile, renderedContent);
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
