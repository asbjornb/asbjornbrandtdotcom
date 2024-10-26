namespace SiteGenerator;

public interface IFolderReader
{
    IAsyncEnumerable<ContentFile> GetFileContents(string folderPath);
    IAsyncEnumerable<ContentFile> GetFileContents(string folderPath, string searchPattern);
}
