namespace SiteGenerator.Templates.MetadataModels;

public record LayoutModel(
    string MetaTitle,
    string MetaDescription,
    string MetaType,
    string PageUrl,
    string SiteAuthor,
    string? Body
);
