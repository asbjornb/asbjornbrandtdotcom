using SiteGenerator.Configuration;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Templates;

public class LayoutModelFactory
{
    private readonly SiteMetadata _siteMetadata;

    public LayoutModelFactory(SiteMetadata siteMetadata)
    {
        _siteMetadata = siteMetadata;
    }

    public LayoutModel CreatePage(string pageUrl, string bodyHtml)
    {
        return new LayoutModel(
            _siteMetadata.SiteTitle,
            _siteMetadata.Description,
            "website",
            pageUrl,
            bodyHtml
        );
    }

    public LayoutModel CreateNote(string noteTitle, string pageUrl)
    {
        return new LayoutModel(
            $"{noteTitle} • {_siteMetadata.Author}'s Notes",
            _siteMetadata.Description,
            "article",
            pageUrl,
            null
        );
    }

    public LayoutModel CreatePost(string postTitle, string pageUrl)
    {
        return new LayoutModel(
            $"{postTitle} • {_siteMetadata.SiteTitle}",
            _siteMetadata.Description,
            "article",
            pageUrl,
            null
        );
    }

    public LayoutModel CreatePostsIndex(string pageUrl)
    {
        return new LayoutModel(
            $"Posts • {_siteMetadata.SiteTitle}",
            _siteMetadata.Description,
            "website",
            pageUrl,
            null
        );
    }
}
