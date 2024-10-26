using System.Text.RegularExpressions;

namespace SiteGenerator.BacklinksProcessing;

public class BacklinkCollector
{
    private readonly Dictionary<string, List<string>> _backlinks = [];

    public async Task CollectBacklinksAsync(IFolderReader folderReader, string contentPath)
    {
        var noteFiles = folderReader.GetFileContents(contentPath, "*.md");
        await foreach (var file in noteFiles)
        {
            var matches = Regex.Matches(file.Content, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                var linkedNote = match.Groups[1].Value;
                var currentNote = Path.GetFileNameWithoutExtension(file.Name);
                if (!_backlinks.TryGetValue(linkedNote, out var value))
                {
                    value = [];
                    _backlinks[linkedNote] = value;
                }

                value.Add(currentNote);
            }
        }
    }

    public IEnumerable<string> GetBacklinksForNote(string noteName)
    {
        return _backlinks.TryGetValue(noteName, out var links) ? links : Enumerable.Empty<string>();
    }
}
