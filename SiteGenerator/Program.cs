namespace SiteGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Site Generation...");

        string contentDirectory = "../content";
        string outputDirectory = "../output";

        var generator = new Generator(contentDirectory, outputDirectory);
        await generator.GenerateSiteAsync();

        Console.WriteLine("Site Generation Complete!");
    }
}