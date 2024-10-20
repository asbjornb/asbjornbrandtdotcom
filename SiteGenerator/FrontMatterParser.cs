using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace SiteGenerator;

public static class FrontMatterParser
{
    public static (Dictionary<string, object> frontMatter, string content) ExtractFrontMatter(
        string markdownContent
    )
    {
        var regex = new Regex(@"^---\s*(.*?)\s*---\s*(.*)", RegexOptions.Singleline);
        var match = regex.Match(markdownContent);

        if (match.Success)
        {
            var yamlContent = match.Groups[1].Value;
            var content = match.Groups[2].Value;
            var frontMatter = ParseYaml(yamlContent);
            return (frontMatter, content);
        }
        else
        {
            return (new Dictionary<string, object>(), markdownContent);
        }
    }

    private static Dictionary<string, object> ParseYaml(string yamlContent)
    {
        var deserializer = new DeserializerBuilder().Build();
        return deserializer.Deserialize<Dictionary<string, object>>(yamlContent);
    }
}
