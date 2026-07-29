namespace RecettesFamille.Rag.Configuration;

/// <summary>
/// Configuration options for the RecettesFamille RAG system.
/// </summary>
public class RagOptions
{
    /// <summary>
    /// PostgreSQL connection string with pgvector support.
    /// Example: "Host=localhost;Port=5432;Database=chatapp1;Username=user;Password=pass"
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// OpenAI API key for embeddings and chat.
    /// </summary>
    public required string OpenAIKey { get; set; }

    /// <summary>
    /// OpenAI embedding model to use.
    /// Default: "text-embedding-3-small" (1536 dimensions, cost-effective).
    /// Alternative: "text-embedding-3-large" (3072 dimensions, higher quality).
    /// </summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// OpenAI chat model to use.
    /// Default: "gpt-4o-mini" (fast and cost-effective).
    /// Alternatives: "gpt-4o", "gpt-4-turbo", etc.
    /// </summary>
    public string ChatModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Name of the vector collection (table) in PostgreSQL.
    /// Default: "RagRecipeEmbeddings".
    /// </summary>
    public string VectorCollectionName { get; set; } = "RagRecipeEmbeddings";

    /// <summary>
    /// Dimension of the embedding vectors.
    /// Must match the EmbeddingModel:
    /// - text-embedding-3-small: 1536
    /// - text-embedding-3-large: 3072
    /// </summary>
    public int EmbeddingDimension { get; set; } = 1536;

    /// <summary>
    /// Minimum similarity score threshold for search results (0.0 to 1.0).
    /// Default: 0.40 (reasonable relevance threshold).
    /// Higher values = stricter filtering, lower values = more results.
    /// </summary>
    public double SearchScoreThreshold { get; set; } = 0.40;

    /// <summary>
    /// Maximum number of search results to return.
    /// Default: 10.
    /// </summary>
    public int MaxSearchResults { get; set; } = 10;
}
