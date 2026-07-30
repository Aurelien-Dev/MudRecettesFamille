using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using RecettesFamille.Rag.Ingestion;
using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Services;

/// <summary>
/// Result of a resync operation with statistics.
/// </summary>
/// <param name="TotalEntities">Total number of entities processed</param>
/// <param name="IngestedCount">Number of entities actually ingested</param>
/// <param name="SkippedCount">Number of entities skipped (no changes detected)</param>
/// <param name="TotalChunks">Total number of chunks created</param>
public record ResyncResult(int TotalEntities, int IngestedCount, int SkippedCount, int TotalChunks);

/// <summary>
/// Service for managing RAG vector ingestion on a per-entity basis.
/// Supports real-time ingestion, updates, and deletions triggered by domain events.
/// </summary>
public class RagIngestionService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly VectorStore _vectorStore;
    private readonly IngestionCacheDbContext _cacheContext;
    private readonly ILogger<RagIngestionService> _logger;
    private readonly string _collectionName;

    public RagIngestionService(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        VectorStore vectorStore,
        IngestionCacheDbContext cacheContext,
        ILogger<RagIngestionService> logger,
        string collectionName = "RagRecipeEmbeddings")
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _cacheContext = cacheContext ?? throw new ArgumentNullException(nameof(cacheContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collectionName = collectionName;
    }

    /// <summary>
    /// Ingests a single entity into the RAG system (CREATE scenario).
    /// Generates embeddings and stores them in the vector store and cache.
    /// Forces ingestion as this is for newly created entities.
    /// </summary>
    /// <param name="entity">The entity to ingest (must implement IIngestionEntity)</param>
    /// <param name="entityPrefix">Prefix for the key (e.g., "recipe", "document")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task IngestSingleAsync<TEntity>(
        TEntity entity,
        string entityPrefix = "recipe",
        CancellationToken cancellationToken = default)
        where TEntity : IIngestionEntity
    {
        await IngestEntityWithCacheAsync(entity, entityPrefix, cancellationToken);
    }

    /// <summary>
    /// Updates an existing entity in the RAG system (UPDATE scenario).
    /// Uses intelligent ingestion: only regenerates embeddings if content has changed (detected via hash).
    /// </summary>
    /// <param name="entity">The entity to update (must implement IIngestionEntity)</param>
    /// <param name="entityPrefix">Prefix for the key (e.g., "recipe", "document")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity was updated (content changed), false if skipped (no changes)</returns>
    public async Task<bool> UpdateSingleAsync<TEntity>(
        TEntity entity,
        string entityPrefix = "recipe",
        CancellationToken cancellationToken = default)
        where TEntity : IIngestionEntity
    {
        return await IngestIfChangedAsync(entity, entityPrefix, cancellationToken);
    }

    /// <summary>
    /// Deletes an entity from the RAG system (DELETE scenario).
    /// Removes the document from cache (cascade deletes records) and all chunks from vector store.
    /// </summary>
    /// <param name="entityId">Entity ID to delete</param>
    /// <param name="entityPrefix">Prefix for the key (e.g., "recipe", "document")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteSingleAsync(int entityId, string entityPrefix = "recipe", CancellationToken cancellationToken = default)
    {
        var sourceId = $"{entityPrefix}_{entityId}";

        _logger.LogInformation("🗑️  Deleting {SourceId}...", sourceId);

        // Find and delete document from cache (cascade will delete records)
        var document = await _cacheContext.Documents
            .Include(d => d.Records)
            .FirstOrDefaultAsync(d => d.SourceId == sourceId, cancellationToken);

        if (document != null)
        {
            var recordKeys = document.Records.Select(r => r.Id).ToList();

            // Delete from vector store
            var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);
            if (await vectorCollection.CollectionExistsAsync(cancellationToken))
            {
                if (recordKeys.Count > 0)
                {
                    await vectorCollection.DeleteAsync(recordKeys, cancellationToken);
                }
            }

            // Delete from cache (cascade deletes records)
            _cacheContext.Documents.Remove(document);
            await _cacheContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Deleted {SourceId} successfully ({RecordCount} chunks)", sourceId, recordKeys.Count);
        }
        else
        {
            _logger.LogWarning("⚠️  Document {SourceId} not found in cache, nothing to delete", sourceId);
        }
    }

    /// <summary>
    /// Private method to ingest an entity with full cache management (force ingestion).
    /// Generates chunks, embeddings, creates/updates document and records in cache, and upserts to vector store.
    /// Does NOT check if content has changed - always ingests.
    /// </summary>
    private async Task IngestEntityWithCacheAsync<TEntity>(
        TEntity entity,
        string entityPrefix,
        CancellationToken cancellationToken = default)
        where TEntity : IIngestionEntity
    {
        var sourceId = $"{entityPrefix}_{entity.Id}";
        var currentHash = entity.CalculateContentHash();
        var chunks = entity.GetSearchableChunks();

        _logger.LogInformation("🔄 Ingesting {SourceId} with {ChunkCount} chunks...", sourceId, chunks.Count);

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);
        await vectorCollection.EnsureCollectionExistsAsync(cancellationToken);

        // Delete existing records from vector store if they exist
        var existingDocument = await _cacheContext.Documents
            .Include(d => d.Records)
            .FirstOrDefaultAsync(d => d.SourceId == sourceId, cancellationToken);

        if (existingDocument != null)
        {
            // Delete old vector embeddings
            var oldRecordKeys = existingDocument.Records.Select(r => r.Id).ToList();
            if (oldRecordKeys.Count > 0)
            {
                await vectorCollection.DeleteAsync(oldRecordKeys, cancellationToken);
            }

            // Remove old records from cache
            _cacheContext.Records.RemoveRange(existingDocument.Records);
            existingDocument.Records.Clear();
            existingDocument.Version = currentHash;
        }
        else
        {
            // Create new document in cache
            existingDocument = new IngestedDocument
            {
                Id = sourceId,
                SourceId = sourceId,
                Version = currentHash
            };
            _cacheContext.Documents.Add(existingDocument);
        }

        await _cacheContext.SaveChangesAsync(cancellationToken);

        // Generate embeddings and create records for each chunk
        int chunksProcessed = 0;
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var recordId = $"{entityPrefix}_{entity.Id}_chunk_{i}";

            // Generate embedding
            var embeddings = await _embeddingGenerator.GenerateAsync([chunk.Text], cancellationToken: cancellationToken);

            // Create vector record
            var vectorRecord = new SemanticSearchRecord
            {
                Key = recordId,
                RecipeId = entity.Id,
                ChunkType = chunk.ChunkType,
                Category = entity.GetCategory(),
                Text = chunk.Text,
                Vector = embeddings[0].Vector
            };

            // Upsert to vector store
            await vectorCollection.UpsertAsync(vectorRecord, cancellationToken);

            // Create cache record
            var cacheRecord = new IngestedRecord
            {
                Id = recordId,
                DocumentId = existingDocument.Id
            };
            _cacheContext.Records.Add(cacheRecord);
            chunksProcessed++;
        }

        await _cacheContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("✅ Ingested {SourceId} successfully ({ChunkCount} chunks)", sourceId, chunksProcessed);
    }

    /// <summary>
    /// Private method to ingest an entity only if content has changed (intelligent ingestion).
    /// Checks if the document exists in cache and compares hash to detect changes.
    /// Only calls IngestEntityWithCacheAsync if the content has actually changed.
    /// </summary>
    /// <returns>True if ingestion occurred, false if skipped (no changes detected)</returns>
    private async Task<bool> IngestIfChangedAsync<TEntity>(
        TEntity entity,
        string entityPrefix,
        CancellationToken cancellationToken = default)
        where TEntity : IIngestionEntity
    {
        var sourceId = $"{entityPrefix}_{entity.Id}";
        var currentHash = entity.CalculateContentHash();

        // Check if document exists in cache
        var existingDocument = await _cacheContext.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.SourceId == sourceId, cancellationToken);

        if (existingDocument != null && existingDocument.Version == currentHash)
        {
            // Content hasn't changed, skip ingestion
            _logger.LogDebug("⏭️  Skipping {SourceId} - content unchanged (hash: {Hash})", sourceId, currentHash);
            return false;
        }

        // Content has changed or document doesn't exist, ingest it
        await IngestEntityWithCacheAsync(entity, entityPrefix, cancellationToken);
        return true;
    }

    /// <summary>
    /// Force re-synchronizes all entities from the database (ADMIN scenario - COMPLETE RESET).
    /// Deletes everything (vector store + cache) and re-ingests all entities.
    /// Use this when the RAG system is completely out of sync or corrupted.
    /// For incremental updates, use ResyncAllAsync instead.
    /// </summary>
    /// <typeparam name="TEntity">Entity type implementing IIngestionEntity</typeparam>
    /// <param name="entities">Queryable of entities to ingest (with all necessary includes)</param>
    /// <param name="entityPrefix">Prefix for keys (e.g., "recipe")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ResyncResult with statistics</returns>
    public async Task<ResyncResult> ForceResyncAllAsync<TEntity>(
        IQueryable<TEntity> entities,
        string entityPrefix = "recipe",
        CancellationToken cancellationToken = default) 
        where TEntity : class, IIngestionEntity
    {
        _logger.LogWarning("🚨 Starting FORCE resync (complete reset) for {EntityType}...", typeof(TEntity).Name);

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);

        // Recreate vector collection (clears all vector data)
        if (await vectorCollection.CollectionExistsAsync(cancellationToken))
        {
            _logger.LogInformation("Deleting existing vector collection {Collection}", _collectionName);
            await vectorCollection.EnsureCollectionDeletedAsync(cancellationToken);
        }

        await vectorCollection.EnsureCollectionExistsAsync(cancellationToken);
        _logger.LogInformation("Created fresh vector collection {Collection}", _collectionName);

        // Clear cache tables
        _logger.LogInformation("Clearing cache tables...");
        await _cacheContext.Database.ExecuteSqlRawAsync(
            "DELETE FROM \"RagIngestionRecords\"; DELETE FROM \"RagIngestionDocuments\";",
            cancellationToken);

        // Load all entities
        var entityList = await entities.ToListAsync(cancellationToken);
        _logger.LogInformation("📊 Found {Count} entities to force ingest", entityList.Count);

        // Ingest each entity using IngestEntityWithCacheAsync
        int successCount = 0;
        int totalChunks = 0;

        foreach (var entity in entityList)
        {
            try
            {
                var chunkCount = entity.GetSearchableChunks().Count;
                await IngestEntityWithCacheAsync(entity, entityPrefix, cancellationToken);
                totalChunks += chunkCount;
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️  Failed to force ingest entity {Id}", entity.Id);
            }
        }

        _logger.LogWarning("✅ Force resync completed: {EntityCount} entities, {ChunkCount} total chunks", successCount, totalChunks);
        return new ResyncResult(entityList.Count, successCount, 0, totalChunks);
    }

    /// <summary>
    /// Intelligent re-synchronizes all entities from the database (ADMIN scenario - INCREMENTAL UPDATE).
    /// Only ingests entities that have changed (detected via hash comparison).
    /// Does NOT delete existing data - performs incremental updates only.
    /// Use this for regular maintenance when most data is already up-to-date.
    /// </summary>
    /// <typeparam name="TEntity">Entity type implementing IIngestionEntity</typeparam>
    /// <param name="entities">Queryable of entities to ingest (with all necessary includes)</param>
    /// <param name="entityPrefix">Prefix for keys (e.g., "recipe")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ResyncResult with statistics (total, ingested, skipped)</returns>
    public async Task<ResyncResult> ResyncAllAsync<TEntity>(
        IQueryable<TEntity> entities,
        string entityPrefix = "recipe",
        CancellationToken cancellationToken = default) 
        where TEntity : class, IIngestionEntity
    {
        _logger.LogWarning("🔄 Starting intelligent resync (incremental) for {EntityType}...", typeof(TEntity).Name);

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);
        await vectorCollection.EnsureCollectionExistsAsync(cancellationToken);

        // Load all entities
        var entityList = await entities.ToListAsync(cancellationToken);
        _logger.LogInformation("📊 Found {Count} entities to process", entityList.Count);

        // Ingest each entity using IngestIfChangedAsync (intelligent)
        int ingestedCount = 0;
        int skippedCount = 0;
        int totalChunks = 0;

        foreach (var entity in entityList)
        {
            try
            {
                bool wasIngested = await IngestIfChangedAsync(entity, entityPrefix, cancellationToken);
                if (wasIngested)
                {
                    var chunkCount = entity.GetSearchableChunks().Count;
                    totalChunks += chunkCount;
                    ingestedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️  Failed to ingest entity {Id}", entity.Id);
            }
        }

        _logger.LogWarning("✅ Intelligent resync completed: {Total} total, {Ingested} ingested, {Skipped} skipped, {ChunkCount} chunks",
            entityList.Count, ingestedCount, skippedCount, totalChunks);
        return new ResyncResult(entityList.Count, ingestedCount, skippedCount, totalChunks);
    }
}