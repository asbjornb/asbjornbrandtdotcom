namespace SiteGenerator.Templates.MetadataModels;

public record NoteModel(string Content, IReadOnlyList<BacklinkModel> Backlinks);
