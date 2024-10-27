namespace SiteGenerator.Processors;

public interface IPageProcessor
{
    Task ProcessAsync(string inputPath, string outputPath);
}
