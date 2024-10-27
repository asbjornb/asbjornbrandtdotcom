using System.Text.RegularExpressions;

namespace SiteGenerator.BacklinksProcessing;

public class BacklinkCollector
{
    public static async Task<Backlinks> CollectBacklinksAsync(
        IFileProvider folderReader,
        string contentPath
    )
    {
        var backlinks = new Backlinks();
        var noteFiles = folderReader.GetFileContents(contentPath, "*.md");

        await foreach (var file in noteFiles)
        {
            var matches = Regex.Matches(file.Content, @"\[\[(.*?)\]\]");
            var currentNote = Path.GetFileNameWithoutExtension(file.Name);
            foreach (Match match in matches)
            {
                var linkedNote = match.Groups[1].Value;
                backlinks.AddBacklink(linkedNote, currentNote);
            }
        }

        return backlinks;
    }
}
