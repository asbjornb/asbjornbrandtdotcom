namespace SiteGenerator;

public class FolderReader : IFolderReader
{
    public async IAsyncEnumerable<ContentFile> GetFileContents(string folderPath)
    {
        foreach (var filePath in Directory.EnumerateFiles(folderPath))
        {
            string content = await File.ReadAllTextAsync(filePath);
            yield return new ContentFile(Path.GetFileName(filePath), content);
        }
    }
}
