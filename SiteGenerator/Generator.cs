using System;
using System.IO;
using System.Threading.Tasks;

namespace SiteGenerator
{
    public class Generator
    {
        private readonly string _contentDirectory;
        private readonly string _outputDirectory;
        private readonly MarkdownParser _markdownParser;

        public Generator(string contentDirectory, string outputDirectory)
        {
            _contentDirectory = contentDirectory;
            _outputDirectory = outputDirectory;
            _markdownParser = new MarkdownParser();
        }

        public async Task GenerateSiteAsync()
        {
            Directory.CreateDirectory(_outputDirectory);

            foreach (var file in Directory.GetFiles(_contentDirectory, "*.md", SearchOption.AllDirectories))
            {
                await ProcessFileAsync(file);
            }
        }

        private async Task ProcessFileAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var html = _markdownParser.ParseToHtml(content);

            var relativePath = Path.GetRelativePath(_contentDirectory, filePath);
            var outputPath = Path.Combine(_outputDirectory, Path.ChangeExtension(relativePath, ".html"));

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            await File.WriteAllTextAsync(outputPath, html);

            Console.WriteLine($"Processed: {relativePath}");
        }
    }
}
