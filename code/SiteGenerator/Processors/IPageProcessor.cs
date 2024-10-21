namespace SiteGenerator.Processors;

public interface IPageProcessor
{
    Task ProcessAsync(string inputFile, string outputPath);
}
