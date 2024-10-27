namespace SiteGenerator;

public interface IMarkdownConverter
{
    string ConvertToHtml(string markdown);
}
