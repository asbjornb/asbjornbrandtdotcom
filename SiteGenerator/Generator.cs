using System;
using System.IO;
using System.Threading.Tasks;

namespace SiteGenerator
{
    public class Generator
    {
        private readonly string _contentDirectory;
        private readonly string _outputDirectory;
        private readonly string _templateDirectory;
        private readonly MarkdownParser _markdownParser;
        private readonly TemplateRenderer _templateRenderer;

        public Generator(string contentDirectory, string outputDirectory, string templateDirectory)
        {
            _contentDirectory = contentDirectory;
            _outputDirectory = outputDirectory;
            _templateDirectory = templateDirectory;
            _markdownParser = new MarkdownParser();
            _templateRenderer = new TemplateRenderer();
        }

        public async Task GenerateSiteAsync()
        {
            Directory.CreateDirectory(_outputDirectory);

            foreach (
                var file in Directory.GetFiles(
                    _contentDirectory,
                    "*.md",
                    SearchOption.AllDirectories
                )
            )
            {
                await ProcessFileAsync(file);
            }
        }

        private async Task ProcessFileAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var (frontMatter, markdownContent) = FrontMatterParser.ExtractFrontMatter(content);
            var html = _markdownParser.ParseToHtml(markdownContent);

            var relativePath = Path.GetRelativePath(_contentDirectory, filePath);
            var outputPath = Path.Combine(
                _outputDirectory,
                Path.ChangeExtension(relativePath, ".html")
            );

            var templatePath = Path.Combine(_templateDirectory, "default.html");
            var renderedContent = _templateRenderer.RenderTemplate(
                templatePath,
                new { Content = html, Metadata = frontMatter }
            );

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            await File.WriteAllTextAsync(outputPath, renderedContent);

            Console.WriteLine($"Processed: {relativePath}");
        }
    }
}
