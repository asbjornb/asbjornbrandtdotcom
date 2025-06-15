using HandlebarsDotNet;
using SiteGenerator.Templates.MetadataModels;

namespace SiteGenerator.Templates;

public class TemplateRenderer
{
    private readonly IHandlebars _handlebarsContext;
    private readonly ITemplateProvider _templateProvider;

    // Compiled templates cache
    private readonly HandlebarsTemplate<object, object> _layoutTemplate;
    private readonly HandlebarsTemplate<object, object> _noteTemplate;
    private readonly HandlebarsTemplate<object, object> _postTemplate;
    private readonly HandlebarsTemplate<object, object> _postsIndexTemplate;

    public TemplateRenderer(ITemplateProvider templateProvider)
    {
        _templateProvider = templateProvider;
        _handlebarsContext = Handlebars.Create();

        // Register date formatting helper
        _handlebarsContext.RegisterHelper(
            "formatDate",
            (writer, context, parameters) =>
            {
                if (parameters.Length > 0 && parameters[0] is DateTime date)
                {
                    writer.WriteSafeString(date.ToString("MMMM d, yyyy"));
                }
            }
        );

        //Register partials
        string headPartialContent = _templateProvider.GetPartialContent("head");
        _handlebarsContext.RegisterTemplate("head", headPartialContent);

        var backlinksPartialContent = _templateProvider.GetPartialContent("backlinks");
        _handlebarsContext.RegisterTemplate("backlinks", backlinksPartialContent);

        // Compile templates once and cache them
        _layoutTemplate = _handlebarsContext.Compile(
            _templateProvider.GetTemplateContent("layout")
        );
        _noteTemplate = _handlebarsContext.Compile(_templateProvider.GetTemplateContent("note"));
        _postTemplate = _handlebarsContext.Compile(_templateProvider.GetTemplateContent("post"));
        _postsIndexTemplate = _handlebarsContext.Compile(
            _templateProvider.GetTemplateContent("posts-index")
        );
    }

    public string RenderNote(NoteModel noteData, LayoutModel layoutData)
    {
        var noteContent = _noteTemplate(noteData);
        var layout = layoutData with { Body = noteContent };
        return _layoutTemplate(layout);
    }

    public string RenderPage(LayoutModel layoutData)
    {
        if (layoutData.Body == null)
        {
            throw new ArgumentException("Body must be provided in layout data for Page.");
        }
        return _layoutTemplate(layoutData);
    }

    public string RenderPost(PostModel postData, LayoutModel layoutData)
    {
        var postContent = _postTemplate(postData);
        var layout = layoutData with { Body = postContent };
        return _layoutTemplate(layout);
    }

    public string RenderPostsIndex(PostsIndexModel postsData, LayoutModel layoutData)
    {
        var postsContent = _postsIndexTemplate(postsData);
        var layout = layoutData with { Body = postsContent };
        return _layoutTemplate(layout);
    }
}
