using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;

namespace SiteGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Site Generation...");
        var stopwatch = Stopwatch.StartNew();

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddCommandLine(args)
            .Build();

        // Bind configuration sections to records
        var siteMetadata = configuration.GetSection("SiteMetadata").Get<SiteMetadata>();
        var pathsConfig = configuration.GetSection("Paths").Get<PathsConfig>();
        if (siteMetadata is null || pathsConfig is null)
        {
            throw new Exception("Could not bind configuration sections to records.");
        }

        //Paths - auto-detect BasePath
        var basePath = PathResolver.ResolveBasePath();
        var contentPath = Path.Combine(basePath, pathsConfig.ContentDirectory);
        var outputPath = Path.Combine(basePath, pathsConfig.OutputDirectory);
        var templatePath = Path.Combine(basePath, pathsConfig.TemplateDirectory);

        // Validate paths
        ValidatePath(contentPath, nameof(pathsConfig.ContentDirectory));
        ValidatePath(outputPath, nameof(pathsConfig.OutputDirectory));
        ValidatePath(templatePath, nameof(pathsConfig.TemplateDirectory));

        // Run the generator
        var generator = new Generator(
            contentPath,
            outputPath,
            new(new FileTemplateProvider(templatePath)),
            siteMetadata
        );
        await generator.GenerateSiteAsync();

        stopwatch.Stop();
        Console.WriteLine($"Site Generation Complete! Elapsed time: {stopwatch.Elapsed}");
    }

    private static void ValidatePath(string path, string pathName)
    {
        if (!Directory.Exists(path) && !File.Exists(path))
        {
            throw new DirectoryNotFoundException(
                $"The {pathName} at path '{path}' does not exist."
            );
        }
    }
}
