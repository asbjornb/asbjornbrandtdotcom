using Microsoft.Extensions.Configuration;
using SiteGenerator.Configuration;
using SiteGenerator.Templates;
using Xunit;

namespace SiteGenerator.Tests.Helpers;

public class SiteGenerationFixture : IAsyncLifetime
{
    public const string InputPath = "TestData/OldSiteInput";
    public const string OutputPath = "TestOutput";

    public async Task InitializeAsync()
    {
        // Clean up any existing output
        if (Directory.Exists(OutputPath))
        {
            Directory.Delete(OutputPath, true);
        }

        Directory.CreateDirectory(OutputPath);

        // Set up and run site generation
        var siteMetadata =
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("SiteMetadata")
                .Get<SiteMetadata>()
            ?? throw new Exception("Could not bind configuration sections to records.");

        var templateRenderer = new TemplateRenderer(new FileTemplateProvider("TestData/templates"));
        var generator = new Generator(InputPath, OutputPath, templateRenderer, siteMetadata);

        await generator.GenerateSiteAsync();
    }

    public Task DisposeAsync()
    {
        // Clean up test output
        try
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
        }
        catch
        {
            // Ignore cleanup failures
        }

        return Task.CompletedTask;
    }
}
