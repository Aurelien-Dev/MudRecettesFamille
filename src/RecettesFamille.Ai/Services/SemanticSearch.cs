namespace RecettesFamille.Ai.Services;

public class SemanticSearch
{
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly IVectorStore _vectorStore;

    public SemanticSearch(IEmbeddingGenerator embeddingGenerator, IVectorStore vectorStore)
    {
        _embeddingGenerator = embeddingGenerator;
        _vectorStore = vectorStore;
    }

    public async Task<IReadOnlyList<SemanticSearchRecord>> SearchAsync(
        string text,
        int? recipeIdFilter = null,
        string? recipeNameFilter = null,
        string? tagFilter = null,
        string? ingredientFilter = null,
        int maxResults = 5)
    {
        var queryEmbedding = await _embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");
        var nearest = await vectorCollection.VectorizedSearchAsync(queryEmbedding, maxResults, record =>
            (!string.IsNullOrEmpty(record.Ingredients) && record.Ingredients.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(record.Instructions) && record.Instructions.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(record.RecipeName) && record.RecipeName.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(record.Tags) && record.Tags.Contains(text, StringComparison.OrdinalIgnoreCase))
        );
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
        var queryEmbedding = await _embeddingGenerator.GenerateVectorAsync(text);
        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>("data-chatapp2-ingested");
        var nearest = await vectorCollection.VectorizedSearchAsync(queryEmbedding, maxResults, record =>
            (!string.IsNullOrEmpty(record.Ingredients) && record.Ingredients.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(record.Instructions) && record.Instructions.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(record.RecipeName) && record.RecipeName.Contains(text, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrEmpty(record.Tags) && record.Tags.Contains(text, StringComparison.OrdinalIgnoreCase))
        );
        var results = new List<SemanticSearchRecord>();
        await foreach (var item in nearest.Results)
        {
            results.Add(item.Record);
        }
        return results;
    }
}