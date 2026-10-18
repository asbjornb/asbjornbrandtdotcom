using SiteGenerator.Processors;

namespace SiteGenerator.Configuration;

public class SiteUrlResolver
{
    private readonly SiteMetadata _siteMetadata;

    public SiteUrlResolver(SiteMetadata siteMetadata)
    {
        _siteMetadata = siteMetadata;
    }

    private string BaseUrl => _siteMetadata.BaseUrl.TrimEnd('/');

    public string Note(string slug) =>
        IsIndex(slug) ? $"{BaseUrl}/notes/" : $"{BaseUrl}/notes/{slug}/";

    public string Page(string slug)
    {
        if (IsIndex(slug))
        {
            return BaseUrl;
        }

        return MarkdownPageWriter.IsNotFoundPage(slug)
            ? $"{BaseUrl}/{MarkdownPageWriter.NotFoundFileName}"
            : $"{BaseUrl}/{slug}/";
    }

    public string Post(string slug) => $"{BaseUrl}/posts/{slug}/";

    public string PostsIndex() => $"{BaseUrl}/posts/";

    public string PostsFeed() => $"{BaseUrl}/posts/feed.xml";

    public string Sitemap() => $"{BaseUrl}/sitemap.xml";

    private static bool IsIndex(string slug) =>
        slug.Equals("index", StringComparison.OrdinalIgnoreCase);
}
