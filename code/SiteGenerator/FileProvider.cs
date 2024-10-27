namespace SiteGenerator;

public class FileProvider : IFileProvider
{
    public async IAsyncEnumerable<ContentFile> GetFileContents(string folderPath)
    {
        foreach (var filePath in Directory.EnumerateFiles(folderPath))
        {
            string content = await File.ReadAllTextAsync(filePath);
            yield return new ContentFile(Path.GetFileName(filePath), content);
        }
    }

    public async IAsyncEnumerable<ContentFile> GetFileContents(
        string folderPath,
        string searchPattern
    )
    {
        foreach (var filePath in Directory.EnumerateFiles(folderPath, searchPattern))
        {
            string content = await File.ReadAllTextAsync(filePath);
            yield return new ContentFile(Path.GetFileName(filePath), content);
        }
    }
}
