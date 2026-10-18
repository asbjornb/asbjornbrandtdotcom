using System.Text.RegularExpressions;

namespace SiteGenerator;

public static partial class WikiLinks
{
    [GeneratedRegex(@"\[\[(.+?)\]\]")]
    public static partial Regex Pattern();

    public static IEnumerable<string> Targets(string markdown) =>
        Pattern().Matches(markdown).Select(match => match.Groups[1].Value);
}
