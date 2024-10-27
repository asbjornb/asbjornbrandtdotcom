using System.Text.RegularExpressions;

namespace SiteGenerator.BacklinksProcessing;

public class BacklinkCollector
{
    public static async Task<Backlinks> CollectBacklinksAsync(
        IFolderReader folderReader,
        string contentPath
    )
    {
        var backlinks = new Dictionary<string, List<string>>();
        var noteFiles = folderReader.GetFileContents(contentPath, "*.md");

        await foreach (var file in noteFiles)
        {
            var matches = Regex.Matches(file.Content, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                var linkedNote = match.Groups[1].Value;
                var currentNote = Path.GetFileNameWithoutExtension(file.Name);
                if (!backlinks.TryGetValue(linkedNote, out var value))
                {
                    value = new List<string>();
                    backlinks[linkedNote] = value;
                }

                value.Add(currentNote);
            }
        }

        return new Backlinks(backlinks);
    }
}
