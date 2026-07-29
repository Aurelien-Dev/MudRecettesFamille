using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Ingestion;

/// <summary>
/// Generic ingestion source that reads entities from a DbContext.
/// Works with any entity type that implements IIngestionEntity.
/// </summary>
/// <typeparam name="TEntity">The entity type to ingest (must implement IIngestionEntity).</typeparam>
public class DatabaseIngestionSource<TEntity> : IIngestionSource
    where TEntity : class, IIngestionEntity
{
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;
    private readonly string _entityTypeName;
    private readonly string _entityPrefix;

    /// <summary>
    /// Initializes a new instance of DatabaseIngestionSource.
    /// </summary>
    /// <param name="dbContext">DbContext containing the entities to ingest.</param>
    /// <param name="entityTypeName">Name of the entity type (e.g., "Recipe", "Document"). Used for SourceId.</param>
    /// <param name="entityPrefix">Prefix for document IDs (e.g., "recipe", "document"). Defaults to lowercase entity type name.</param>
    public DatabaseIngestionSource(DbContext dbContext, string entityTypeName, string? entityPrefix = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = dbContext.Set<TEntity>();
        _entityTypeName = entityTypeName ?? throw new ArgumentNullException(nameof(entityTypeName));
        _entityPrefix = entityPrefix ?? entityTypeName.ToLowerInvariant();
    }

    public string SourceId => $"DatabaseIngestionSource:{_entityTypeName}";

    public async Task<IEnumerable<IngestedDocument>> GetNewOrModifiedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments)
    {
        var results = new List<IngestedDocument>();
        var entities = await _dbSet.ToListAsync();

        foreach (var entity in entities)
        {
            var documentId = $"{_entityPrefix}_{entity.Id}";
            var contentHash = entity.CalculateContentHash();

            var existingDocument = await existingDocuments
                .Where(d => d.SourceId == SourceId && d.Id == documentId)
                .FirstOrDefaultAsync();

            if (existingDocument is null)
            {
                // New entity
                results.Add(new IngestedDocument
                {
                    Id = documentId,
                    Version = contentHash,
                    SourceId = SourceId
                });
            }
            else if (existingDocument.Version != contentHash)
            {
                // Modified entity
                existingDocument.Version = contentHash;
                results.Add(existingDocument);
            }
        }

        return results;
    }

    public async Task<IEnumerable<IngestedDocument>> GetDeletedDocumentsAsync(IQueryable<IngestedDocument> existingDocuments)
    {
        var entityIds = await _dbSet.Select(e => $"{_entityPrefix}_{e.Id}").ToListAsync();
        return await existingDocuments
            .Where(d => d.SourceId == SourceId && !entityIds.Contains(d.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<SemanticSearchRecord>> CreateRecordsForDocumentAsync(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        string documentId)
    {
        // Extract entity ID from document ID (format: "prefix_5")
        var entityIdStr = documentId.Replace($"{_entityPrefix}_", "");
        if (!int.TryParse(entityIdStr, out var entityId))
        {
            throw new InvalidOperationException($"Invalid document ID format: {documentId}");
        }

        // Load the entity from database
        var entity = await _dbSet.FindAsync(entityId);
        if (entity is null)
        {
            throw new InvalidOperationException($"{_entityTypeName} with ID {entityId} not found");
        }

        // Get searchable content and create specialized chunks
        var chunks = entity.GetSearchableChunks();
        var category = entity.GetCategory();

        var records = new List<SemanticSearchRecord>();

        // Generate embeddings for each chunk
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embeddings = await embeddingGenerator.GenerateAsync([chunk.Text]);

            var record = new SemanticSearchRecord
            {
                Key = $"{documentId}_chunk_{i}",
                RecipeId = entityId,
                ChunkType = chunk.ChunkType,
                Category = category,
                Text = chunk.Text,
                Vector = embeddings[0].Vector
            };

            records.Add(record);
        }

        return records;
    }
}
