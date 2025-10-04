namespace SiteGenerator.Configuration;

public class SiteUrlResolver
{
    private readonly SiteMetadata _siteMetadata;

    public SiteUrlResolver(SiteMetadata siteMetadata)
    {
        _siteMetadata = siteMetadata;
    }

    private string BaseUrl => _siteMetadata.BaseUrl.TrimEnd('/');

    public string Note(string slug) => $"{BaseUrl}/notes/{slug}/";

    public string Page(string slug)
    {
        return slug.Equals("index", StringComparison.OrdinalIgnoreCase)
            ? BaseUrl
            : $"{BaseUrl}/{slug}/";
    }

    public string Post(string slug) => $"{BaseUrl}/posts/{slug}/";

    public string PostsIndex() => $"{BaseUrl}/posts/";
}
