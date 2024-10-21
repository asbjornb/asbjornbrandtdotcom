using System.Text.RegularExpressions;
using Markdig;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SiteGenerator;

public class Generator
{
    private readonly string _contentPath;
    private readonly string _outputPath;
    private readonly string _templatePath;
    private readonly string _configPath;
    private readonly TemplateRenderer _templateRenderer;
    private readonly Dictionary<string, List<string>> _backlinks = [];

    public Generator(string contentPath, string outputPath, string templatePath, string configPath)
    {
        _contentPath = contentPath;
        _outputPath = outputPath;
        _templatePath = templatePath;
        _configPath = configPath;
        _templateRenderer = new TemplateRenderer(_templatePath);
    }

    public async Task GenerateSiteAsync()
    {
        // First pass: collect backlinks
        await CollectBacklinksAsync();

        // Second pass: generate pages
        await GeneratePagesAsync();
    }

    private async Task CollectBacklinksAsync()
    {
        var noteFiles = Directory.GetFiles(Path.Combine(_contentPath, "notes"), "*.md");
        foreach (var file in noteFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            var matches = Regex.Matches(content, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                var linkedNote = match.Groups[1].Value;
                var currentNote = Path.GetFileNameWithoutExtension(file);
                if (!_backlinks.TryGetValue(linkedNote, out var value))
                {
                    value = [];
                    _backlinks[linkedNote] = value;
                }

                value.Add(currentNote);
            }
        }
    }

    private async Task GeneratePagesAsync()
    {
        // Generate notes
        var noteFiles = Directory.GetFiles(Path.Combine(_contentPath, "notes"), "*.md");
        foreach (var file in noteFiles)
        {
            await GeneratePageAsync(file, "note", "notes");
        }

        // Generate pages
        var pageFiles = Directory.GetFiles(Path.Combine(_contentPath, "pages"), "*.md");
        foreach (var file in pageFiles)
        {
            await GeneratePageAsync(file, "default", "pages");
        }

        // Generate posts (if needed)
        var postFiles = Directory.GetFiles(Path.Combine(_contentPath, "posts"), "*.md");
        foreach (var file in postFiles)
        {
            await GeneratePageAsync(file, "post", "posts");
        }
    }

    private async Task GeneratePageAsync(string inputFile, string templateName, string outputSubdir)
    {
        var content = await File.ReadAllTextAsync(inputFile);
        var (frontMatter, markdownContent) = ExtractFrontMatter(content);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

        var fileName = Path.GetFileNameWithoutExtension(inputFile);
        if (_backlinks.TryGetValue(fileName, out var links))
        {
            htmlContent += "<h2>Backlinks</h2><ul>";
            foreach (var link in links)
            {
                htmlContent += $"<li><a href=\"{link}.html\">{link}</a></li>";
            }
            htmlContent += "</ul>";
        }

        var renderedContent = await _templateRenderer.RenderAsync(
            templateName,
            new { Metadata = frontMatter, Content = htmlContent }
        );

        var outputFile = Path.Combine(
            _outputPath,
            outputSubdir,
            Path.GetFileNameWithoutExtension(inputFile) + ".html"
        );
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile));
        await File.WriteAllTextAsync(outputFile, renderedContent);
    }

    private (Dictionary<string, object> frontMatter, string content) ExtractFrontMatter(
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
