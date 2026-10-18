using System.Text;
using System.Text.RegularExpressions;

namespace SiteGenerator;

public static partial class WikiLinks
{
    [GeneratedRegex(@"\[\[(.+?)\]\]")]
    public static partial Regex Pattern();

    // Fenced code blocks (``` or ~~~) and inline code spans (which can't cross a blank line).
    // Wikilinks inside these are left as written.
    [GeneratedRegex(
        @"^[ \t]*(?<fence>`{3,}|~{3,}).*?(?:^[ \t]*\k<fence>[ \t]*$|\z)|(?<ticks>`+)(?!`)(?:(?!\n[ \t]*\n).)+?(?<!`)\k<ticks>(?!`)",
        RegexOptions.Multiline | RegexOptions.Singleline
    )]
    private static partial Regex CodePattern();

    public static IEnumerable<string> Targets(string markdown) =>
        Pattern()
            .Matches(CodePattern().Replace(markdown, ""))
            .Select(match => match.Groups[1].Value);

    /// <summary>Replaces wikilinks outside code blocks and code spans.</summary>
    public static string Replace(string markdown, MatchEvaluator evaluator)
    {
        var result = new StringBuilder();
        var position = 0;
        foreach (Match code in CodePattern().Matches(markdown))
        {
            result.Append(Pattern().Replace(markdown[position..code.Index], evaluator));
            result.Append(code.Value);
            position = code.Index + code.Length;
        }
        result.Append(Pattern().Replace(markdown[position..], evaluator));
        return result.ToString();
    }
}
