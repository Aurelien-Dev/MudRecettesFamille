using Microsoft.Extensions.AI;
using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Ingestion;

/// <summary>
/// Interface for ingestion sources that provide documents to be indexed in the RAG system.
/// Implementations can read from databases, file systems, APIs, etc.
/// </summary>
public interface IIngestionSource
{
    /// <summary>
    /// Unique identifier for this ingestion source (e.g., "DatabaseIngestionSource:Recipes").
    /// </summary>
    string SourceId { get; }

    /// <summary>
    /// Gets documents that are new or have been modified since last ingestion.
    /// </summary>
    /// <param name="existingDocuments">Queryable of documents already ingested from this source.</param>
    /// <returns>Documents that need to be (re-)ingested.</returns>
    Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments);

    /// <summary>
    /// Gets documents that have been deleted from the source and should be removed from the index.
    /// </summary>
    /// <param name="existingDocuments">Queryable of documents already ingested from this source.</param>
    /// <returns>Documents that should be deleted from the index.</returns>
    Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments);

    /// <summary>
    /// Creates semantic search records (chunks) for a specific document.
    /// Generates embeddings for each chunk.
    /// </summary>
    /// <param name="embeddingGenerator">Embedding generator to create vectors.</param>
    /// <param name="documentId">ID of the document to process.</param>
    /// <returns>Collection of semantic search records with embeddings.</returns>
    Task<IEnumerable<SemanticSearchRecord>> CreateRecordsForDocumentAsync(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, string documentId);
}
