namespace SiteGenerator.Templates.MetadataModels;

public record PostModel(
    string Title,
    string Content,
    DateTime PublishedDate,
    string Slug,
    string Url
);

public record PostsIndexModel(IReadOnlyList<PostSummaryModel> Posts);

public record PostSummaryModel(string Title, string Url, DateTime PublishedDate, string Slug);
