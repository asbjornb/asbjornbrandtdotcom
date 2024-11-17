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

    public async Task WriteFileAsync(string filePath, string content)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (directory != null)
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(filePath, content);
    }

    public void CopyFolderAsync(string sourceFolder, string destinationFolder)
    {
        Directory.CreateDirectory(destinationFolder);
        foreach (
            string dir in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories)
        )
        {
            Directory.CreateDirectory(
                Path.Combine(destinationFolder, dir[(sourceFolder.Length + 1)..])
            );
        }

        foreach (
            string file_name in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories)
        )
        {
            var destinationPath = Path.Combine(
                destinationFolder,
                file_name[(sourceFolder.Length + 1)..]
            );
            File.Copy(file_name, destinationPath, true);
        }
    }
}
