using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SiteGenerator;

public class BacklinkCollector
{
    private readonly Dictionary<string, List<string>> _backlinks = [];

    public async Task CollectBacklinksAsync(string contentPath)
    {
        var noteFiles = Directory.GetFiles(Path.Combine(contentPath, "notes"), "*.md");
        foreach (var file in noteFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            var matches = Regex.Matches(content, @"\[\[(.*?)\]\]");
            foreach (Match match in matches)
            {
                var linkedNote = match.Groups[1].Value;
                var currentNote = Path.GetFileNameWithoutExtension(file);
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
