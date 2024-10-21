namespace SiteGenerator;

public interface IPageProcessor
{
    Task ProcessAsync(string inputFile, string outputPath);
}
