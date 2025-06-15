using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace RecettesFamille.Ai.Services;

public class SemanticSearch(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IVectorStore vectorStore)
{
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchAsync(
        string text,
        int? recipeIdFilter = null,
        string? recipeNameFilter = null,
        string? tagFilter = null,
        string? ingredientFilter = null,
        int maxResults = 5)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");

        var nearest = await vectorCollection.VectorizedSearchAsync(queryEmbedding, new VectorSearchOptions<SemanticSearchRecord>
        {
            Top = maxResults,
            Filter = record =>
                (!string.IsNullOrEmpty(record.Ingredients) && record.Ingredients.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(record.Instructions) && record.Instructions.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(record.RecipeName) && record.RecipeName.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(record.Tags) && record.Tags.Contains(text, StringComparison.OrdinalIgnoreCase))
        });

        var results = new List<SemanticSearchRecord>();
        await foreach (var item in nearest.Results)
        {
            results.Add(item.Record);
        }

        return results;
    }
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchLightAsync(
        string text,
        int maxResults = 5)
    {
        var queryEmbedding = await embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");

        // Recherche vectorielle avec filtre textuel
        var nearest = await vectorCollection.VectorizedSearchAsync(queryEmbedding, new VectorSearchOptions<SemanticSearchRecord>
        {
            Top = maxResults,
            Filter = record =>
                (!string.IsNullOrEmpty(record.Ingredients) && record.Ingredients.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(record.Instructions) && record.Instructions.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(record.RecipeName) && record.RecipeName.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(record.Tags) && record.Tags.Contains(text, StringComparison.OrdinalIgnoreCase))
        });

        var results = new List<SemanticSearchRecord>();
        await foreach (var item in nearest.Results)
        {
            results.Add(item.Record);
        }

        return results;
    }

}
