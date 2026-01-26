using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RecettesFamille.Ai.Services.Ingestion;

public class DataIngestor(
    ILogger<DataIngestor> logger,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    IVectorStore vectorStore,
    IngestionCacheDbContext ingestionCacheDb)
{
    public static async Task IngestDataAsync(IServiceProvider services, IIngestionSource source)
    {
        using var scope = services.CreateScope();
        var ingestor = scope.ServiceProvider.GetRequiredService<DataIngestor>();
        await ingestor.IngestDataAsync(source);
    }

    public async Task IngestDataAsync(IIngestionSource source)
    {
        _ = await ingestionCacheDb.Database.EnsureCreatedAsync();

        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");
        await vectorCollection.CreateCollectionIfNotExistsAsync();

        var documentsForSource = ingestionCacheDb.Documents
            .Where(d => d.SourceId == source.SourceId)
            .Include(d => d.Records);

        var deletedFiles = await source.GetDeletedDocumentsAsync(documentsForSource);
        foreach (var deletedFile in deletedFiles)
        {
            logger.LogInformation("Removing ingested data for {file}", deletedFile.Id);
            await vectorCollection.DeleteBatchAsync(deletedFile.Records.Select(r => r.Id));
            ingestionCacheDb.Documents.Remove(deletedFile);
        }
        await ingestionCacheDb.SaveChangesAsync();

        var modifiedDocs = await source.GetNewOrModifiedDocumentsAsync(documentsForSource);
        foreach (var modifiedDoc in modifiedDocs)
        {
            logger.LogInformation("Processing {file}", modifiedDoc.Id);

            if (modifiedDoc.Records.Count > 0)
            {
                await vectorCollection.DeleteBatchAsync(modifiedDoc.Records.Select(r => r.Id));
            }

            IEnumerable<SemanticSearchRecord> newRecords;
            try
            {
                newRecords = await source.CreateRecordsForDocumentAsync(embeddingGenerator, modifiedDoc.Id)
                             ?? Enumerable.Empty<SemanticSearchRecord>();
            }
            catch (EmptyRecipeException)
            {
                continue;
            }

            await foreach (var id in vectorCollection.UpsertBatchAsync(newRecords)) { }

            modifiedDoc.Records.Clear();
            modifiedDoc.Records.AddRange(newRecords.Select(r => new IngestedRecord { Id = r.Key, DocumentId = modifiedDoc.Id }));

            if (ingestionCacheDb.Entry(modifiedDoc).State == EntityState.Detached)
            {
                ingestionCacheDb.Documents.Add(modifiedDoc);
            }
        }

        await ingestionCacheDb.SaveChangesAsync();
        logger.LogInformation("Ingestion is up-to-date");
    }

    public static async Task RefreshRecipeAsync(IServiceProvider services, IIngestionSource source, int recipeId)
    {
        using var scope = services.CreateScope();
        var ingestor = scope.ServiceProvider.GetRequiredService<DataIngestor>();
        await ingestor.RefreshRecipeAsync(source, recipeId);
    }

    public async Task RefreshRecipeAsync(IIngestionSource source, int recipeId)
    {
        _ = await ingestionCacheDb.Database.EnsureCreatedAsync();

        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");
        await vectorCollection.CreateCollectionIfNotExistsAsync();

        var document = await ingestionCacheDb.Documents
            .Where(d => d.SourceId == source.SourceId && d.Id == recipeId)
            .Include(d => d.Records)
            .FirstOrDefaultAsync();

        if (document is not null && document.Records.Count > 0)
        {
            await vectorCollection.DeleteBatchAsync(document.Records.Select(r => r.Id));
            document.Records.Clear();
        }

        IEnumerable<SemanticSearchRecord> newRecords;
        try
        {
            newRecords = await source.CreateRecordsForDocumentAsync(embeddingGenerator, recipeId)
                         ?? Enumerable.Empty<SemanticSearchRecord>();
        }
        catch (EmptyRecipeException)
        {
            // Lenient failure policy: don't fail the recipe save if the recipe is empty.
            logger.LogWarning("Skipping vector refresh for recipe {RecipeId} because it contains no indexable content.", recipeId);
            return;
        }
        catch (Exception ex)
        {
            // Lenient failure policy: log and continue.
            logger.LogError(ex, "Vector refresh failed for recipe {RecipeId}.", recipeId);
            return;
        }

        await foreach (var _ in vectorCollection.UpsertBatchAsync(newRecords)) { }

        document ??= new IngestedDocument { Id = recipeId, SourceId = source.SourceId, Version = string.Empty };
        document.Records.AddRange(newRecords.Select(r => new IngestedRecord { Id = r.Key, DocumentId = document.Id }));

        if (ingestionCacheDb.Entry(document).State == EntityState.Detached)
        {
            ingestionCacheDb.Documents.Add(document);
        }

        await ingestionCacheDb.SaveChangesAsync();
        logger.LogInformation("Vector refresh complete for recipe {RecipeId}", recipeId);
    }
}
