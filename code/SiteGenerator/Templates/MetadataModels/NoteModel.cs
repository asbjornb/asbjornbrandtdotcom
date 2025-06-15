using SiteGenerator.KnowledgeGraph;

namespace SiteGenerator.Templates.MetadataModels;

public record NoteModel(
    string Content,
    IReadOnlyList<BacklinkModel> Backlinks,
    NoteGraphData? GraphData = null
);

public record NoteGraphData(
    string CurrentNoteId,
    List<GraphNode> ConnectedNodes,
    List<GraphLink> Connections,
    string Category,
    NodeType NodeType
);
