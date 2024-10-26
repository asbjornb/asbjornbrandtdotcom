namespace SiteGenerator;

public interface IFolderReader
{
    IAsyncEnumerable<ContentFile> GetFileContents(string folderPath);
}
