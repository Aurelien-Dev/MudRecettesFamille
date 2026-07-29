using Microsoft.Extensions.AI;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Rag.Services;

namespace RecettesFamille.Services;

/// <summary>
/// Service wrapper for RAG operations on recipes.
/// Provides semantic search and contextual chat functionality.
/// </summary>
public class RecipeRagService
{
    private readonly RagSearchService<RecipeEntity> _ragSearch;
    private readonly RagChatService _ragChat;
    private readonly RagIngestionService _ragIngestion;

    public RecipeRagService(
        RagSearchService<RecipeEntity> ragSearch,
        RagChatService ragChat,
        RagIngestionService ragIngestion)
    {
        _ragSearch = ragSearch ?? throw new ArgumentNullException(nameof(ragSearch));
        _ragChat = ragChat ?? throw new ArgumentNullException(nameof(ragChat));
        _ragIngestion = ragIngestion ?? throw new ArgumentNullException(nameof(ragIngestion));
    }

    /// <summary>
    /// Search for recipes using semantic search with natural language queries.
    /// Examples: "poulet rapide", "dessert chocolat", "recette végétarienne"
    /// </summary>
    /// <param name="query">Natural language search query</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="scoreThreshold">Minimum similarity score (0.0 to 1.0)</param>
    /// <returns>List of matching recipes ordered by relevance</returns>
    public async Task<List<RecipeEntity>> SearchRecipesAsync(
        string query,
        int maxResults = 10,
        double scoreThreshold = 0.35)
    {
        return await _ragSearch.SearchByTextAsync(query, maxResults, scoreThreshold);
    }

    /// <summary>
    /// Ask a question about a specific recipe with conversation history.
    /// Uses the complete recipe context (ingredients, instructions, metadata) to provide helpful answers.
    /// </summary>
    /// <param name="recipeId">The recipe ID to ask about</param>
    /// <param name="question">User's question in natural language</param>
    /// <param name="history">Previous conversation messages (optional)</param>
    /// <returns>Assistant's response</returns>
    public async Task<string> ChatWithRecipeAsync(
        int recipeId,
        string question,
        List<ChatMessage>? history = null)
    {
        var systemPrompt = @"Tu es un assistant culinaire expert et bienveillant. 
Tu aides les utilisateurs à comprendre et réussir leurs recettes.

Contexte de la recette :
{0}

Instructions :
- Réponds UNIQUEMENT avec les informations présentes dans le contexte de la recette
- Sois précis, clair et encourageant
- Si la question concerne quelque chose qui n'est pas dans la recette, dis-le poliment
- Propose des conseils pratiques et des alternatives si pertinent
- Utilise un ton amical et professionnel
- Réponds en français";

        return await _ragChat.AskQuestionAsync(
            recipeId,
            question,
            systemPrompt,
            emptyContextMessage: "Je n'ai pas trouvé d'informations sur cette recette.",
            invalidQuestionMessage: "Je n'ai pas compris votre question. Pouvez-vous la reformuler ?",
            history);
    }

    /// <summary>
    /// Manually ingests a single recipe into the RAG system.
    /// Typically called automatically by repository hooks, but exposed for manual operations.
    /// </summary>
    public async Task IngestRecipeAsync(RecipeEntity recipe)
    {
        await _ragIngestion.IngestSingleAsync(
            recipe.Id,
            recipe.GetSearchableContent(),
            recipe.GetCategory(),
            "recipe");
    }

    /// <summary>
    /// Updates an existing recipe in the RAG system.
    /// Typically called automatically by repository hooks.
    /// </summary>
    public async Task UpdateRecipeAsync(RecipeEntity recipe)
    {
        await _ragIngestion.UpdateSingleAsync(
            recipe.Id,
            recipe.GetSearchableContent(),
            recipe.GetCategory(),
            "recipe");
    }

    /// <summary>
    /// Deletes a recipe from the RAG system.
    /// Typically called automatically by repository hooks.
    /// </summary>
    public async Task DeleteRecipeAsync(int recipeId)
    {
        await _ragIngestion.DeleteSingleAsync(recipeId, "recipe");
    }
}
