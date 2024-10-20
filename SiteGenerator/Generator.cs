using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SiteGenerator
{
    public class Config
    {
        public string SiteTitle { get; set; }
        public string BaseUrl { get; set; }
    }

    public class Generator
    {
        private readonly string _contentDirectory;
        private readonly string _outputDirectory;
        private readonly string _templateDirectory;
        private readonly string _configPath;
        private readonly MarkdownParser _markdownParser;
        private readonly TemplateRenderer _templateRenderer;
        private Config _config;
        private Dictionary<string, List<string>> _backlinks;
        private Dictionary<string, string> _noteMapping;

        public Generator(
            string contentDirectory,
            string outputDirectory,
            string templateDirectory,
            string configPath
        )
        {
            _contentDirectory = contentDirectory;
            _outputDirectory = outputDirectory;
            _templateDirectory = templateDirectory;
            _configPath = configPath;
            _markdownParser = new MarkdownParser();
            _templateRenderer = new TemplateRenderer();
            _backlinks = new Dictionary<string, List<string>>();
            _noteMapping = new Dictionary<string, string>();
            LoadConfig();
        }

        private void LoadConfig()
        {
            string configContent = File.ReadAllText(_configPath);
            _config = JsonConvert.DeserializeObject<Config>(configContent);
        }

        public async Task GenerateSiteAsync()
        {
            Directory.CreateDirectory(_outputDirectory);

            await ProcessPagesAsync();
            await ProcessPostsAsync();
            await ProcessNotesAsync();
            CopyAssets();
        }

        private async Task ProcessPagesAsync()
        {
            var pagesDirectory = Path.Combine(_contentDirectory, "pages");
            if (Directory.Exists(pagesDirectory))
            {
                await ProcessContentFilesAsync(pagesDirectory, "page.html");
            }
        }

        private async Task ProcessPostsAsync()
        {
            var postsDirectory = Path.Combine(_contentDirectory, "posts");
            if (Directory.Exists(postsDirectory))
            {
                await ProcessContentFilesAsync(postsDirectory, "post.html");
            }
        }

        private async Task ProcessNotesAsync()
        {
            var notesDirectory = Path.Combine(_contentDirectory, "notes");
            if (Directory.Exists(notesDirectory))
            {
                // First pass: build note mapping
                foreach (var file in Directory.GetFiles(notesDirectory, "*.md"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    _noteMapping[fileName] = $"notes/{fileName}.html";
                }

                // Second pass: process notes and build backlinks
                foreach (var file in Directory.GetFiles(notesDirectory, "*.md"))
                {
                    await ProcessNoteAsync(file);
                }

                // Third pass: update notes with backlinks
                foreach (var file in Directory.GetFiles(notesDirectory, "*.md"))
                {
                    await UpdateNoteWithBacklinksAsync(file);
                }
            }
        }

        private async Task ProcessNoteAsync(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var (frontMatter, markdownContent) = FrontMatterParser.ExtractFrontMatter(content);
            var html = _markdownParser.ParseToHtml(markdownContent, _noteMapping);

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            foreach (var note in _noteMapping.Keys)
            {
                if (markdownContent.Contains($"[[{note}]]"))
                {
                    if (!_backlinks.ContainsKey(note))
                    {
                        _backlinks[note] = new List<string>();
                    }
                    _backlinks[note].Add(fileName);
                }
            }

            var relativePath = Path.GetRelativePath(_contentDirectory, filePath);
            var outputPath = Path.Combine(
                _outputDirectory,
                Path.ChangeExtension(relativePath, ".html")
            );

            var templatePath = Path.Combine(_templateDirectory, "note.html");
            if (!File.Exists(templatePath))
            {
                templatePath = Path.Combine(_templateDirectory, "default.html");
            }

            var renderedContent = _templateRenderer.RenderTemplate(
                templatePath,
                new
                {
                    Content = html,
                    Metadata = frontMatter,
                    Config = _config
                }
            );

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            await File.WriteAllTextAsync(outputPath, renderedContent);

            Console.WriteLine($"Processed note: {relativePath}");
        }

        private async Task UpdateNoteWithBacklinksAsync(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var outputPath = Path.Combine(_outputDirectory, "notes", $"{fileName}.html");

            if (File.Exists(outputPath))
            {
                var content = await File.ReadAllTextAsync(outputPath);
                var backlinksHtml = "";

                if (_backlinks.ContainsKey(fileName))
                {
                    backlinksHtml = "<h2>Backlinks</h2><ul>";
                    foreach (var backlink in _backlinks[fileName])
                    {
                        backlinksHtml +=
                            $"<li><a href=\"{_noteMapping[backlink]}\">{backlink}</a></li>";
                    }
                    backlinksHtml += "</ul>";
                }

                content = content.Replace("</body>", $"{backlinksHtml}</body>");
                await File.WriteAllTextAsync(outputPath, content);
            }
        }

        private async Task ProcessContentFilesAsync(string directory, string templateName)
        {
            foreach (var file in Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories))
            {
                await ProcessFileAsync(file, templateName);
            }
        }

        private async Task ProcessFileAsync(string filePath, string templateName)
        {
            var content = await File.ReadAllTextAsync(filePath);
            var (frontMatter, markdownContent) = FrontMatterParser.ExtractFrontMatter(content);
            var html = _markdownParser.ParseToHtml(markdownContent, _noteMapping);

            var relativePath = Path.GetRelativePath(_contentDirectory, filePath);
            var outputPath = Path.Combine(
                _outputDirectory,
                Path.ChangeExtension(relativePath, ".html")
            );

            var templatePath = Path.Combine(_templateDirectory, templateName);
            if (!File.Exists(templatePath))
            {
                templatePath = Path.Combine(_templateDirectory, "default.html");
            }

            var renderedContent = _templateRenderer.RenderTemplate(
                templatePath,
                new
                {
                    Content = html,
                    Metadata = frontMatter,
                    Config = _config
                }
            );

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            await File.WriteAllTextAsync(outputPath, renderedContent);

            Console.WriteLine($"Processed: {relativePath}");
        }

        private void CopyAssets()
        {
            var assetsDirectory = Path.Combine(_contentDirectory, "assets");
            var outputAssetsDirectory = Path.Combine(_outputDirectory, "assets");

            if (Directory.Exists(assetsDirectory))
            {
                CopyDirectory(assetsDirectory, outputAssetsDirectory);
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                var destDirectory = Path.Combine(destinationDir, Path.GetFileName(directory));
                CopyDirectory(directory, destDirectory);
            }
        }
    }
}
