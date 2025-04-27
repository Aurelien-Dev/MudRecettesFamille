using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace RecettesFamille.Ai.Services;

public class SemanticSearch(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, IVectorStore vectorStore)
{
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchAsync(string text, string? recipeNameFilter, int maxResults)
    {
        var queryEmbedding = await embeddingGenerator.GenerateEmbeddingVectorAsync(text);
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");

        var nearest = await vectorCollection.VectorizedSearchAsync(queryEmbedding, new VectorSearchOptions<SemanticSearchRecord>
        {
            Top = maxResults,
            Filter = recipeNameFilter is { Length: > 0 } ? record => record.RecipeName == recipeNameFilter : null,
        });
        var results = new List<SemanticSearchRecord>();
        await foreach (var item in nearest.Results)
        {
            results.Add(item.Record);
        }

        return results;
    }
}
