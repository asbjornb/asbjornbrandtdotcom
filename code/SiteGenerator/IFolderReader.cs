namespace SiteGenerator;

public interface IFolderReader
{
    IAsyncEnumerable<File> GetFileContents();
}
