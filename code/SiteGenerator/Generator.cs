﻿using SiteGenerator.Processors;

namespace SiteGenerator;

public class Generator
{
    private readonly string _contentPath;
    private readonly string _outputPath;
    private readonly string _templatePath;
    private readonly string _configPath;
    private readonly TemplateRenderer _templateRenderer;
    private readonly BacklinkCollector _backlinkCollector;
    private readonly Dictionary<string, IPageProcessor> _processors;

    public Generator(string contentPath, string outputPath, string templatePath, string configPath)
    {
        _contentPath = contentPath;
        _outputPath = outputPath;
        _templatePath = templatePath;
        _configPath = configPath;
        _templateRenderer = new TemplateRenderer(_templatePath);
        _backlinkCollector = new BacklinkCollector();

        _processors = new Dictionary<string, IPageProcessor>
        {
            ["notes"] = new NoteProcessor(_backlinkCollector, _templateRenderer),
            ["pages"] = new PageProcessor(_templateRenderer),
            ["posts"] = new PostProcessor(_templateRenderer)
        };
    }

    public async Task GenerateSiteAsync()
    {
        await _backlinkCollector.CollectBacklinksAsync(_contentPath);

        foreach (var (subdir, processor) in _processors)
        {
            var inputPath = Path.Combine(_contentPath, subdir);
            var outputPath = Path.Combine(_outputPath, subdir);
            var files = Directory.GetFiles(inputPath, "*.md");

            foreach (var file in files)
            {
                await processor.ProcessAsync(file, outputPath);
            }
        }
    }
}
