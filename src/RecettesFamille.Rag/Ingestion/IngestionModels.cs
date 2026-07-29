namespace RecettesFamille.Rag.Ingestion;

/// <summary>
/// Represents a document that has been ingested into the RAG system.
/// Tracks the version (hash) of the document to detect changes.
/// </summary>
public class IngestedDocument
{
    // TODO: Make Id+SourceId a composite key
    public required string Id { get; set; }
    public required string SourceId { get; set; }
    public required string Version { get; set; }
    public List<IngestedRecord> Records { get; set; } = [];
}

/// <summary>
/// Represents a record (chunk) that was created from an ingested document.
/// Links to the parent IngestedDocument.
/// </summary>
public class IngestedRecord
{
    public required string Id { get; set; }
    public required string DocumentId { get; set; }
}
