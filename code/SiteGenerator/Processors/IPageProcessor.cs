namespace SiteGenerator.Processors;

public interface IPageProcessor
{
    Task ProcessAsync(IFolderReader folderReader, string inputPath, string outputPath);
}
