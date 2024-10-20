namespace SiteGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Site Generation...");

        string contentDirectory = "../content";
        string outputDirectory = "../output";
        string templateDirectory = "../templates";
        string configPath = "../config/config.json";

        var generator = new Generator(
            contentDirectory,
            outputDirectory,
            templateDirectory,
            configPath
        );
        await generator.GenerateSiteAsync();

        Console.WriteLine("Site Generation Complete!");
    }
}
