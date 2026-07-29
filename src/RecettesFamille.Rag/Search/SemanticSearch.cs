using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Npgsql;
using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Search;

public class SemanticSearch(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    string connectionString)
{
    /// <summary>
    /// Search across all entities without filtering.
    /// Used for global search (Scenario 1).
    /// Returns results with similarity scores.
    /// Workaround: Manually retrieves full Text field from PostgreSQL to avoid truncation bug in CommunityToolkit.VectorData.PgVector v1.0.0
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string text, int maxResults)
    {
        var embeddings = await embeddingGenerator.GenerateAsync([text]);
        var queryEmbedding = embeddings[0].Vector;
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("RagRecipeEmbeddings");

        var results = new List<SearchResult>();
        await foreach (var item in vectorCollection.SearchAsync(queryEmbedding, maxResults))
        {
            results.Add(new SearchResult(item.Record, item.Score ?? 0.0));
        }

        return results;
    }

    /// <summary>
    /// Search within a specific entity only.
    /// Used for contextual chat on an entity detail page (Scenario 2).
    /// Workaround: Manually retrieves full Text field from PostgreSQL to avoid truncation bug.
    /// </summary>
    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchByRecipeIdAsync(string text, int recipeId, int maxResults)
    {
        var embeddings = await embeddingGenerator.GenerateAsync([text]);
        var queryEmbedding = embeddings[0].Vector;
        var vectorCollection = vectorStore.GetCollection<string, SemanticSearchRecord>("RagRecipeEmbeddings");

        var results = new List<SemanticSearchRecord>();
        await foreach (var item in vectorCollection.SearchAsync(queryEmbedding, maxResults, new VectorSearchOptions<SemanticSearchRecord>
        {
            Filter = record => record.RecipeId == recipeId,
        }))
        {
            results.Add(item.Record);
        }
        return results;
    }
}
