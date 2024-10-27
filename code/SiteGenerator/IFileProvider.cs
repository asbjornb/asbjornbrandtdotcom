namespace SiteGenerator;

public interface IFileProvider
{
    IAsyncEnumerable<ContentFile> GetFileContents(string folderPath);
    IAsyncEnumerable<ContentFile> GetFileContents(string folderPath, string searchPattern);
}
