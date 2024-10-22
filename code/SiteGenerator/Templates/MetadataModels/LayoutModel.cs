namespace SiteGenerator.Templates.MetadataModels;

public record LayoutModel(
    string MetaTitle,
    string MetaDescription,
    string MetaType, //MetaType can be "website" or "article". Check OpenGraph protocol for more information.
    string PageUrl,
    string? Body
);
