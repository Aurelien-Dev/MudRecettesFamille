namespace RecettesFamille.Rag.Ingestion;

/// <summary>
/// Interface that must be implemented by entities that can be ingested into the RAG system.
/// This abstraction allows the ingestion pipeline to work with any domain entity (Recipe, Document, Article, etc.).
/// </summary>
public interface IIngestionEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// Used to generate the document ID (e.g., "recipe_5").
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Returns the complete searchable content for this entity.
    /// This text will be used to generate embeddings for semantic search.
    /// </summary>
    /// <returns>Formatted text containing all relevant information for RAG.</returns>
    string GetSearchableContent();

    /// <summary>
    /// Returns specialized chunks for multi-chunk embedding strategy.
    /// Each chunk focuses on a specific aspect of the entity (metadata, content, etc.)
    /// to improve search relevance for different query types.
    /// </summary>
    /// <returns>List of specialized chunks with their types and content.</returns>
    List<SearchableChunk> GetSearchableChunks();

    /// <summary>
    /// Returns the category of this entity for filtering purposes.
    /// Used as a filterable property in the vector store.
    /// </summary>
    /// <returns>Category string (e.g., "Dessert", "Main Course", "Technical Documentation").</returns>
    string GetCategory();

    /// <summary>
    /// Calculates a hash of the entity's content to detect changes.
    /// The ingestion system uses this hash to determine if the entity has been modified
    /// and needs to be re-indexed.
    /// </summary>
    /// <returns>Base64-encoded SHA256 hash of the entity's content.</returns>
    string CalculateContentHash();
}

/// <summary>
/// Represents a specialized chunk of searchable content with a specific type/purpose.
/// </summary>
/// <param name="ChunkType">Type of chunk (e.g., "Metadata", "Description", "Ingredients", "Instructions")</param>
/// <param name="Text">The text content to be embedded</param>
public record SearchableChunk(string ChunkType, string Text);
