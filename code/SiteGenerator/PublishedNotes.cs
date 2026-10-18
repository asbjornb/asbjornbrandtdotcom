namespace SiteGenerator;

/// <summary>
/// Which notes go on the site. Read from <c>published.txt</c> in the content root: one note per
/// line, as a path (<c>thoughts/git.md</c>) or a bare slug (<c>git</c>). Blank lines and lines
/// starting with <c>#</c> are ignored. Without the file, every note is published.
/// </summary>
public class PublishedNotes
{
    public const string FileName = "published.txt";

    // The notes index is the /notes/ landing page, so it is always published
    private const string IndexSlug = "index";

    private readonly HashSet<string>? _slugs;

    private PublishedNotes(HashSet<string>? slugs)
    {
        _slugs = slugs;
    }

    public static PublishedNotes All { get; } = new(null);

    public bool PublishesEverything => _slugs is null;

    public static PublishedNotes Parse(string fileContent)
    {
        var slugs = fileContent
            .Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .Select(line => Path.GetFileNameWithoutExtension(line.Replace('\\', '/').TrimEnd('/')))
            .Where(slug => slug.Length > 0)
            .ToHashSet();

        slugs.Add(IndexSlug);
        return new PublishedNotes(slugs);
    }

    public static async Task<PublishedNotes> LoadAsync(string contentPath)
    {
        var filePath = Path.Combine(contentPath, FileName);
        if (!File.Exists(filePath))
        {
            return All;
        }

        return Parse(await File.ReadAllTextAsync(filePath));
    }

    public bool IsPublished(string slug) => _slugs is null || _slugs.Contains(slug);

    public bool IsPublishedFile(string fileName) =>
        IsPublished(Path.GetFileNameWithoutExtension(fileName));
}
