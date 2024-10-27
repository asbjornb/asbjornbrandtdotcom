namespace SiteGenerator.BacklinksProcessing;

public class Backlinks
{
    private readonly Dictionary<string, HashSet<string>> _backlinks;

    public Backlinks()
    {
        _backlinks = [];
    }

    public void AddBacklink(string linkedNote, string currentNote)
    {
        if (!_backlinks.TryGetValue(linkedNote, out var value))
        {
            value = [];
            _backlinks[linkedNote] = value;
        }

        value.Add(currentNote);
    }

    public IEnumerable<string> GetBacklinksForNote(string noteName)
    {
        return _backlinks.TryGetValue(noteName, out var links) ? links : Enumerable.Empty<string>();
    }

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> AllBacklinks =>
        _backlinks.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyCollection<string>)kvp.Value);
}
