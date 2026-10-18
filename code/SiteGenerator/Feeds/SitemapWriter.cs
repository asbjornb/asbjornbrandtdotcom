using System.Xml.Linq;

namespace SiteGenerator.Feeds;

/// <summary>
/// Builds <c>sitemap.xml</c> (sitemaps.org 0.9) from the URLs of the generated pages.
/// </summary>
public static class SitemapWriter
{
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public static string Build(IEnumerable<string> urls)
    {
        var urlset = new XElement(
            Ns + "urlset",
            urls.Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .Select(url => new XElement(Ns + "url", new XElement(Ns + "loc", url)))
        );

        return new XDeclaration("1.0", "utf-8", null) + Environment.NewLine + urlset;
    }
}
