using System.Reflection;
using SiteGenerator.Templates;

namespace SiteGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Site Generation...");

        // Get the directory of the executing assembly
        string assemblyDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? throw new Exception("Could not determine the executing assembly location.");

        string gitRootDirectory = Path.GetFullPath(
            Path.Combine(assemblyDirectory, "../../../../../")
        );

        string contentDirectory = gitRootDirectory;
        string outputDirectory = Path.Combine(gitRootDirectory, "site-output");
        string templateDirectory = Path.Combine(gitRootDirectory, "code", "templates");
        string configPath = Path.Combine(assemblyDirectory, "config.json");

        Console.WriteLine($"Content Directory: {contentDirectory}");
        Console.WriteLine($"Output Directory: {outputDirectory}");
        Console.WriteLine($"Template Directory: {templateDirectory}");
        Console.WriteLine($"Config Path: {configPath}");

        // Validate paths (optional)
        ValidatePath(contentDirectory, nameof(contentDirectory));
        ValidatePath(outputDirectory, nameof(outputDirectory));
        ValidatePath(templateDirectory, nameof(templateDirectory));
        ValidatePath(configPath, nameof(configPath));

        // Run the generator
        var generator = new Generator(
            contentDirectory,
            outputDirectory,
            new(new FileTemplateProvider(templateDirectory)),
            configPath
        );
        await generator.GenerateSiteAsync();

        Console.WriteLine("Site Generation Complete!");
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
