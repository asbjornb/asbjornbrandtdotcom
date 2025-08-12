namespace SiteGenerator.Templates.MetadataModels;

public record RecentNotesModel(
    IReadOnlyList<RecentNoteItem>? NewestNotes = null,
    IReadOnlyList<RecentNoteItem>? RecentlyChangedNotes = null
);

public record RecentNoteItem(string FileName, string Title);
