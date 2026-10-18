using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SiteGenerator.Configuration;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Feeds;

/// <summary>
/// Builds the RSS 2.0 feed for posts, newest first, with the full post HTML as the description.
/// </summary>
public static class RssFeedWriter
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";

    public static string Build(
        SiteMetadata siteMetadata,
        string postsIndexUrl,
        string feedUrl,
        IEnumerable<PostModel> posts
    )
    {
        var items = posts
            .OrderByDescending(p => p.PublishedDate)
            .Select(post => new XElement(
                "item",
                new XElement("title", PlainText(post.Title)),
                new XElement("link", post.Url),
                new XElement("guid", new XAttribute("isPermaLink", "true"), post.Url),
                new XElement("pubDate", Rfc822(post.PublishedDate)),
                new XElement("description", post.Content)
            ))
            .ToList();

        var channel = new XElement(
            "channel",
            new XElement("title", siteMetadata.SiteTitle),
            new XElement("link", postsIndexUrl),
            new XElement("description", siteMetadata.Description),
            new XElement("language", "en"),
            new XElement(
                Atom + "link",
                new XAttribute("href", feedUrl),
                new XAttribute("rel", "self"),
                new XAttribute("type", "application/rss+xml")
            ),
            items
        );

        var rss = new XElement(
            "rss",
            new XAttribute("version", "2.0"),
            new XAttribute(XNamespace.Xmlns + "atom", Atom),
            channel
        );

        return new XDeclaration("1.0", "utf-8", null) + Environment.NewLine + rss;
    }

    // Post titles come from the rendered <h1>, so they can hold inline tags and entities
    private static string PlainText(string html) =>
        WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", "")).Trim();

    private static string Rfc822(DateTime date) =>
        DateTime
            .SpecifyKind(date, DateTimeKind.Utc)
            .ToString("ddd, dd MMM yyyy HH:mm:ss +0000", CultureInfo.InvariantCulture);
}
