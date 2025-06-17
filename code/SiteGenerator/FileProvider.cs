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

        const int maxRetries = 5;
        int retryDelayMs = 100; // Start with 100ms delay

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Use FileShare.Read to allow other processes to read the file while we're writing
                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.Read
                );

                using var writer = new StreamWriter(fileStream);
                await writer.WriteAsync(content);
                return; // Success - exit the method
            }
            catch (IOException ex) when (IsFileLocked(ex) && attempt < maxRetries - 1)
            {
                // Wait before retrying with exponential backoff
                await Task.Delay(retryDelayMs);
                retryDelayMs *= 2;
            }
        }

        // If we've exhausted all retries, try one last time without catching the exception
        await File.WriteAllTextAsync(filePath, content);
    }

    // Determine if an IOException is related to file locking
    private static bool IsFileLocked(IOException exception)
    {
        return exception.Message.Contains("being used by another process")
            || exception.Message.Contains("access is denied")
            || exception.Message.Contains("The process cannot access the file");
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
