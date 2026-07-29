using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using RecettesFamille.Rag.Ingestion;
using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Services;

/// <summary>
/// Service for managing RAG vector ingestion on a per-entity basis.
/// Supports real-time ingestion, updates, and deletions triggered by domain events.
/// </summary>
public class RagIngestionService
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly VectorStore _vectorStore;
    private readonly ILogger<RagIngestionService> _logger;
    private readonly string _collectionName;

    public RagIngestionService(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        VectorStore vectorStore,
        ILogger<RagIngestionService> logger,
        string collectionName = "RagRecipeEmbeddings")
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _collectionName = collectionName;
    }

    /// <summary>
    /// Ingests a single entity into the RAG system (CREATE scenario).
    /// Generates embeddings and stores them in the vector store.
    /// </summary>
    /// <param name="entityId">Entity ID</param>
    /// <param name="searchableContent">Full searchable text content</param>
    /// <param name="category">Category for filtering</param>
    /// <param name="entityPrefix">Prefix for the key (e.g., "recipe", "document")</param>
    public async Task IngestSingleAsync(int entityId, string searchableContent, string category, string entityPrefix = "recipe")
    {
        var key = $"{entityPrefix}_{entityId}";

        _logger.LogInformation("🔄 Ingesting {Key}...", key);

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);
        await vectorCollection.EnsureCollectionExistsAsync();

        // Generate embedding
        var embeddings = await _embeddingGenerator.GenerateAsync([searchableContent]);

        // Create vector record
        var record = new SemanticSearchRecord
        {
            Key = key,
            RecipeId = entityId,
            ChunkType = "Complete",
            Category = category,
            Text = searchableContent,
            Vector = embeddings[0].Vector
        };

        // Upsert (will insert if new, update if exists)
        await vectorCollection.UpsertAsync(record);

        _logger.LogInformation("✅ Ingested {Key} successfully", key);
    }

    /// <summary>
    /// Updates an existing entity in the RAG system (UPDATE scenario).
    /// Regenerates embeddings with the new content.
    /// </summary>
    public async Task UpdateSingleAsync(int entityId, string searchableContent, string category, string entityPrefix = "recipe")
    {
        var key = $"{entityPrefix}_{entityId}";

        _logger.LogInformation("🔄 Updating {Key}...", key);

        // Upsert handles both insert and update, so same implementation as IngestSingleAsync
        await IngestSingleAsync(entityId, searchableContent, category, entityPrefix);

        _logger.LogInformation("✅ Updated {Key} successfully", key);
    }

    /// <summary>
    /// Deletes an entity from the RAG system (DELETE scenario).
    /// Removes the vector record from the store.
    /// </summary>
    public async Task DeleteSingleAsync(int entityId, string entityPrefix = "recipe")
    {
        var key = $"{entityPrefix}_{entityId}";

        _logger.LogInformation("🗑️  Deleting {Key}...", key);

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);

        // Check if collection exists
        if (await vectorCollection.CollectionExistsAsync())
        {
            await vectorCollection.DeleteAsync(key);
            _logger.LogInformation("✅ Deleted {Key} successfully", key);
        }
        else
        {
            _logger.LogWarning("⚠️  Collection {Collection} does not exist, skipping delete", _collectionName);
        }
    }

    /// <summary>
    /// Re-synchronizes all entities from the database (ADMIN scenario).
    /// Scans the entire database and rebuilds the vector store using multi-chunk strategy.
    /// Should only be called manually by an admin when RAG is out of sync.
    /// </summary>
    /// <typeparam name="TEntity">Entity type implementing IIngestionEntity</typeparam>
    /// <param name="entities">Queryable of entities to ingest (with all necessary includes)</param>
    /// <param name="entityPrefix">Prefix for keys (e.g., "recipe")</param>
    public async Task<int> ResyncAllAsync<TEntity>(IQueryable<TEntity> entities, string entityPrefix = "recipe") 
        where TEntity : class, IIngestionEntity
    {
        _logger.LogWarning("🚨 Starting full RAG resync for {EntityType}...", typeof(TEntity).Name);

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);

        // Recreate collection (clears all data)
        if (await vectorCollection.CollectionExistsAsync())
        {
            _logger.LogInformation("Deleting existing collection {Collection}", _collectionName);
            await vectorCollection.EnsureCollectionDeletedAsync();
        }

        await vectorCollection.EnsureCollectionExistsAsync();
        _logger.LogInformation("Created fresh collection {Collection}", _collectionName);

        // Load all entities
        var entityList = await entities.ToListAsync();
        _logger.LogInformation("📊 Found {Count} entities to ingest", entityList.Count);

        // Ingest each entity with multi-chunk strategy
        int successCount = 0;
        int totalChunks = 0;

        foreach (var entity in entityList)
        {
            try
            {
                // Get all chunks for this entity
                var chunks = entity.GetSearchableChunks();
                _logger.LogInformation("  📄 Processing {EntityId}: {ChunkCount} chunks", entity.Id, chunks.Count);

                // Generate embeddings for each chunk
                for (int i = 0; i < chunks.Count; i++)
                {
                    var chunk = chunks[i];
                    var embeddings = await _embeddingGenerator.GenerateAsync([chunk.Text]);

                    var record = new SemanticSearchRecord
                    {
                        Key = $"{entityPrefix}_{entity.Id}_chunk_{i}",
                        RecipeId = entity.Id,
                        ChunkType = chunk.ChunkType,
                        Category = entity.GetCategory(),
                        Text = chunk.Text,
                        Vector = embeddings[0].Vector
                    };

                    await vectorCollection.UpsertAsync(record);
                    totalChunks++;
                }

                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️  Failed to ingest entity {Id}", entity.Id);
            }
        }

        _logger.LogWarning("✅ Resync completed: {EntityCount} entities, {ChunkCount} total chunks", successCount, totalChunks);
        return successCount;
    }

    /// <summary>
    /// Rebuilds the ingestion cache from existing vector embeddings.
    /// Useful when cache tables were missing but embeddings already exist.
    /// </summary>
    /// <typeparam name="TEntity">Entity type implementing IIngestionEntity</typeparam>
    /// <param name="entities">Queryable of entities to process (with all necessary includes)</param>
    /// <param name="cacheContext">Ingestion cache context</param>
    /// <param name="entityPrefix">Prefix for keys (e.g., "recipe")</param>
    /// <returns>Number of entities processed</returns>
    public async Task<int> RebuildIngestionCacheAsync<TEntity>(
        IQueryable<TEntity> entities,
        IngestionCacheDbContext cacheContext,
        string entityPrefix = "recipe") where TEntity : class, IIngestionEntity
    {
        _logger.LogWarning("🔄 Rebuilding ingestion cache from existing embeddings...");

        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>(_collectionName);

        if (!await vectorCollection.CollectionExistsAsync())
        {
            _logger.LogError("❌ Vector collection does not exist. Nothing to rebuild from.");
            return 0;
        }

        // Load all entities from the main database
        var entityList = await entities.ToListAsync();
        _logger.LogInformation("📊 Found {Count} entities to process", entityList.Count);

        int rebuiltCount = 0;

        foreach (var entity in entityList)
        {
            try
            {
                // Calculate current content hash
                var currentHash = entity.CalculateContentHash();
                var sourceId = $"{entityPrefix}_{entity.Id}";

                // Get chunks for this entity
                var chunks = entity.GetSearchableChunks();

                // Check if embeddings exist in vector store by checking the first chunk key
                var firstChunkKey = $"{entityPrefix}_{entity.Id}_chunk_0";
                var existingRecord = await vectorCollection.GetAsync(firstChunkKey);

                if (existingRecord != null)
                {
                    // Create or update cache document entry
                    var document = await cacheContext.Documents
                        .FirstOrDefaultAsync(d => d.SourceId == sourceId);

                    if (document == null)
                    {
                        document = new IngestedDocument
                        {
                            Id = sourceId, // Use sourceId as the document Id
                            SourceId = sourceId,
                            Version = currentHash
                        };
                        cacheContext.Documents.Add(document);
                        await cacheContext.SaveChangesAsync();
                    }
                    else
                    {
                        document.Version = currentHash;
                    }

                    // Add record entries for each chunk
                    for (int i = 0; i < chunks.Count; i++)
                    {
                        var recordId = $"{entityPrefix}_{entity.Id}_chunk_{i}";

                        if (!await cacheContext.Records.AnyAsync(r => r.Id == recordId))
                        {
                            cacheContext.Records.Add(new IngestedRecord
                            {
                                Id = recordId,
                                DocumentId = document.Id
                            });
                        }
                    }

                    await cacheContext.SaveChangesAsync();
                    rebuiltCount++;

                    _logger.LogInformation("  ✅ Rebuilt cache for {SourceId} ({ChunkCount} chunks)", sourceId, chunks.Count);
                }
                else
                {
                    _logger.LogWarning("  ⚠️  No embeddings found for {SourceId}, skipping", sourceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️  Failed to rebuild cache for entity {Id}", entity.Id);
            }
        }

        _logger.LogWarning("✅ Cache rebuild completed: {Count} entities processed", rebuiltCount);
        return rebuiltCount;
    }
}