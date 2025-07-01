using SiteGenerator.Tests.Helpers;

namespace SiteGenerator.Tests.Helpers;

public class InMemoryFileProvider : IFileProvider
{
    private readonly Dictionary<string, string> _files = new();

    public void AddFile(string fileName, string content)
    {
        _files[fileName] = content;
    }

    public IAsyncEnumerable<ContentFile> GetFileContents(string folderPath)
    {
        return GetFileContents(folderPath, "*");
    }

    public IAsyncEnumerable<ContentFile> GetFileContents(string folderPath, string searchPattern)
    {
        var files = _files.Keys.AsEnumerable();

        if (searchPattern != "*")
        {
            var pattern = searchPattern.Replace("*", ".*").Replace("?", ".");
            var regex = new System.Text.RegularExpressions.Regex(
                pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            files = files.Where(f => regex.IsMatch(Path.GetFileName(f)));
        }

        var contentFiles = files.Select(fileName => new ContentFile(fileName, _files[fileName]));
        return contentFiles.ToAsyncEnumerable();
    }

    public Task WriteFileAsync(string filePath, string content)
    {
        _files[filePath] = content;
        return Task.CompletedTask;
    }

    public void CopyFolderAsync(string sourceFolder, string destinationFolder)
    {
        // Not needed for tests
    }
}
