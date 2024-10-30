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
                var text = node.InnerText;
                int remainingLength = previewLength - currentLength;

                if (remainingLength <= 0)
                {
                    node.ParentNode.RemoveChild(node);
                    reachedLimit = true;
                }
                else if (text.Length > remainingLength)
                {
                    node.InnerHtml =
                        HtmlEntity.Entitize(text.Substring(0, remainingLength)) + "...";
                    currentLength += remainingLength;
                    reachedLimit = true;
                }
                else
                {
                    currentLength += text.Length;
                }
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
}
