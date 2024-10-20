using System;
using System.CommandLine;
using System.IO;
using StaticSiteGenerator;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Static Site Generator CLI");

        var buildCommand = new Command("build", "Build the static site");
        buildCommand.SetHandler(() =>
        {
            Console.WriteLine("Building the static site...");
            var contentDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "content");
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "output");
            var generator = new Generator(contentDir, outputDir);
            generator.GenerateSite();
            Console.WriteLine("Site built successfully.");
        });

        var deployCommand = new Command("deploy", "Deploy the static site to Cloudflare Pages");
        deployCommand.SetHandler(() =>
        {
            Console.WriteLine("Deploying to Cloudflare Pages...");
            // TODO: Implement Cloudflare Pages deployment logic
            Console.WriteLine("Deployment completed.");
        });

        rootCommand.AddCommand(buildCommand);
        rootCommand.AddCommand(deployCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
