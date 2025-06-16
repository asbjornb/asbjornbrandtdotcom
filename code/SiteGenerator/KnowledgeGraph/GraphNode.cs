namespace SiteGenerator.KnowledgeGraph;

public record GraphNode(
    string Id,
    string Title,
    string Url,
    string Category,
    int Size,
    List<string> Headers,
    NodeType Type = NodeType.Note
);

public enum NodeType
{
    Note,
    Category,
    Hub,
}

public record GraphLink(
    string Source,
    string Target,
    LinkType Type = LinkType.Reference,
    int Strength = 1
);

public enum LinkType
{
    Reference, // [[link]] style
    Hierarchical, // parent-child relationship
    Related, // thematic connection
    External // outbound link
    ,
}

public record GraphData(
    List<GraphNode> Nodes,
    List<GraphLink> Links,
    Dictionary<string, int> Categories
);
