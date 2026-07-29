using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RecettesFamille.Rag.Search;
using System.Numerics.Tensors;

namespace RecettesFamille.Rag.Services;

/// <summary>
/// Generic service for searching entities using semantic search (Scenario 1).
/// Works with any DbContext and entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type to search for.</typeparam>
public class RagSearchService<TEntity> where TEntity : class
{
    private readonly SemanticSearch _semanticSearch;
    private readonly DbContext _dbContext;
    private readonly DbSet<TEntity> _dbSet;
    private readonly ILogger<RagSearchService<TEntity>> _logger;

    public RagSearchService(
        SemanticSearch semanticSearch,
        DbContext dbContext,
        ILogger<RagSearchService<TEntity>> logger)
    {
        _semanticSearch = semanticSearch ?? throw new ArgumentNullException(nameof(semanticSearch));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dbSet = dbContext.Set<TEntity>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Search for entities using semantic search with hybrid re-ranking.
    /// Returns complete entities ordered by relevance.
    /// Uses a two-stage approach:
    /// 1. Vector search to get initial chunk candidates
    /// 2. Group by entity and aggregate scores (best chunk wins)
    /// 3. Re-ranking with metadata boost (category matching, text overlap)
    /// </summary>
    /// <param name="query">Search query (e.g., "chicken", "quick desserts", "vegetarian")</param>
    /// <param name="maxResults">Maximum number of entities to return</param>
    /// <param name="scoreThreshold">Minimum similarity score (0.0 to 1.0). Default 0.35 for initial candidates.</param>
    /// <returns>List of entities ordered by relevance score</returns>
    public async Task<List<TEntity>> SearchByTextAsync(string query, int maxResults = 10, double scoreThreshold = 0.35)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new List<TEntity>();
        }

        // Fetch more chunk candidates for re-ranking (4x maxResults since we have ~4 chunks per entity)
        var candidateCount = Math.Max(maxResults * 4, 40);
        var searchResults = await _semanticSearch.SearchAsync(query, candidateCount);

        if (!searchResults.Any())
        {
            return new List<TEntity>();
        }

        // Group chunks by RecipeId and take the best score per entity
        var groupedByEntity = searchResults
            .GroupBy(r => r.Record.RecipeId)
            .Select(g => new EntityGroup(
                RecipeId: g.Key,
                BestChunk: g.OrderByDescending(r => r.Score).First(),
                BestScore: g.Max(r => r.Score),
                AllChunks: g.ToList()
            ))
            .OrderByDescending(g => g.BestScore)
            .ToList();

        // Apply hybrid re-ranking with metadata boost
        var rerankedResults = ApplyHybridReranking(query, groupedByEntity);

        // Log scores for debugging
        _logger.LogInformation("Search query: '{Query}' - Top results after re-ranking:", query);
        foreach (var result in rerankedResults.Take(50))
        {
            _logger.LogInformation("  EntityId={EntityId}, ChunkType={ChunkType}, VectorScore={VectorScore:F4}, FinalScore={FinalScore:F4}, Category={Category}", 
                result.Record.RecipeId, result.Record.ChunkType, result.VectorScore, result.FinalScore, result.Record.Category);
        }

        // Filter by final score threshold and extract entity IDs
        var entityIds = rerankedResults
            .Where(r => r.FinalScore >= scoreThreshold)
            .Take(maxResults)
            .Select(r => r.Record.RecipeId)
            .Distinct() // Ensure uniqueness
            .ToList();

        if (!entityIds.Any())
        {
            return new List<TEntity>();
        }

        // Load complete entities from database
        var entities = await _dbSet
            .Where(e => entityIds.Contains(EF.Property<int>(e, "Id")))
            .ToListAsync();

        // Create a dictionary for fast lookup by Id
        var entityById = entities.ToDictionary(
            e => (int)_dbContext.Entry(e).Property("Id").CurrentValue!);

        // Reorder entities according to search relevance
        var orderedEntities = entityIds
            .Select(id => entityById.TryGetValue(id, out var entity) ? entity : null)
            .Where(e => e != null)
            .ToList();

        return orderedEntities!;
    }

    /// <summary>
    /// Apply hybrid re-ranking combining vector similarity with metadata matching.
    /// Boosts scores for:
    /// - Category exact match (e.g., query "dessert" matches Category "Dessert")
    /// - Text contains query terms across ALL chunks of the entity (case-insensitive)
    /// </summary>
    private List<RerankedResult> ApplyHybridReranking(string query, List<EntityGroup> groupedByEntity)
    {
        var queryLower = query.ToLowerInvariant();
        var queryTerms = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var reranked = new List<RerankedResult>();

        foreach (var group in groupedByEntity)
        {
            var bestChunk = group.BestChunk;
            var allChunks = group.AllChunks;

            var vectorScore = bestChunk.Score;
            var finalScore = vectorScore;

            var categoryLower = bestChunk.Record.Category?.ToLowerInvariant() ?? "";

            // Boost 1: Category exact match (boost by 0.25)
            if (queryTerms.Any(term => categoryLower.Contains(term)))
            {
                finalScore += 0.25;
                _logger.LogDebug("Category boost (+0.25) for EntityId={EntityId}: {Category} matches query '{Query}'", bestChunk.Record.RecipeId, bestChunk.Record.Category, query);
            }

            // Boost 2: Text contains query terms - CHECK ALL CHUNKS (boost by 0.20 per matching term, max 0.50)
            var allTexts = string.Join(" ", allChunks.Select(c => c.Record.Text)).ToLowerInvariant();
            var matchingTermsCount = queryTerms.Count(term => allTexts.Contains(term));
            if (matchingTermsCount > 0)
            {
                var textBoost = Math.Min(matchingTermsCount * 0.20, 0.50);
                finalScore += textBoost;
                _logger.LogDebug("Text boost (+{Boost:F2}) for EntityId={EntityId}: {Count} terms matched across all chunks", textBoost, bestChunk.Record.RecipeId, matchingTermsCount);
            }

            reranked.Add(new RerankedResult(bestChunk.Record, vectorScore, finalScore));
        }

        return reranked.OrderByDescending(r => r.FinalScore).ToList();
    }

    private record EntityGroup(
        int RecipeId,
        SearchResult BestChunk,
        double BestScore,
        List<SearchResult> AllChunks);

    private record RerankedResult(
        RecettesFamille.Rag.Models.SemanticSearchRecord Record,
        double VectorScore,
        double FinalScore);
}
