namespace SiteGenerator.BacklinksProcessing;

public class Backlinks
{
    // _backlinks holds each note's name as a key and a list of notes that reference it as the value.
    private readonly Dictionary<string, List<string>> _backlinks;

    public Backlinks(Dictionary<string, List<string>> backlinks)
    {
        _backlinks = backlinks;
    }

    public IEnumerable<string> GetBacklinksForNote(string noteName)
    {
        return _backlinks.TryGetValue(noteName, out var links) ? links : Enumerable.Empty<string>();
    }

    public IReadOnlyDictionary<string, List<string>> AllBacklinks => _backlinks;
}
