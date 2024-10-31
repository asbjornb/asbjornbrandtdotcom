using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace SiteGenerator.Previews;

public class PreviewGenerator
{
    public static string GeneratePreview(string htmlContent)
    {
        const int previewLength = 200;
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        int currentLength = 0;
        bool reachedLimit = false;

        // Recursive function to traverse and truncate nodes
        void TruncateNode(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                var html = node.InnerHtml;
                var tokens = TokenizeHtmlEntities(html);
                var newHtmlContent = "";

                foreach (var token in tokens)
                {
                    int tokenLength = HtmlEntity.DeEntitize(token).Length;

                    if (currentLength + tokenLength > previewLength)
                    {
                        reachedLimit = true;
                        break;
                    }
                    else if (currentLength + tokenLength == previewLength)
                    {
                        // If the token is a space, don't include it
                        if (HtmlEntity.DeEntitize(token) == " ")
                        {
                            reachedLimit = true;
                            break;
                        }
                        else
                        {
                            newHtmlContent += token;
                            currentLength += tokenLength;
                            reachedLimit = true;
                            break;
                        }
                    }
                    else
                    {
                        newHtmlContent += token;
                        currentLength += tokenLength;
                    }
                }

                if (reachedLimit)
                {
                    newHtmlContent += "...";
                }

                node.InnerHtml = newHtmlContent;
            }
            else if (node.HasChildNodes)
            {
                foreach (var child in node.ChildNodes.ToList())
                {
                    if (reachedLimit)
                    {
                        node.RemoveChild(child);
                    }
                    else
                    {
                        TruncateNode(child);
                    }
                }
            }
        }

        TruncateNode(doc.DocumentNode);

        return doc.DocumentNode.InnerHtml;
    }

    // Helper method to tokenize text into characters and entities
    private static List<string> TokenizeHtmlEntities(string html)
    {
        var tokens = new List<string>();
        var regex = new Regex(@"&\w+;|&#\d+;|&#x[0-9a-fA-F]+;|.", RegexOptions.Singleline);
        var matches = regex.Matches(html);

        foreach (Match match in matches)
        {
            tokens.Add(match.Value);
        }

        return tokens;
    }
}
