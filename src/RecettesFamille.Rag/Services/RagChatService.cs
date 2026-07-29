using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using RecettesFamille.Rag.Models;

namespace RecettesFamille.Rag.Services;

/// <summary>
/// Generic service for contextual chat on a specific entity (Scenario 2).
/// Loads all chunks for an entity to provide complete context for the conversation.
/// </summary>
public class RagChatService
{
    private readonly VectorStore _vectorStore;
    private readonly IChatClient _chatClient;
    private readonly string _entityPrefix;

    /// <summary>
    /// Initializes a new instance of RagChatService.
    /// </summary>
    /// <param name="vectorStore">Vector store to retrieve entity context.</param>
    /// <param name="chatClient">Chat client for LLM interactions.</param>
    /// <param name="entityPrefix">Prefix for entity keys (e.g., "recipe", "document"). Defaults to "recipe".</param>
    public RagChatService(VectorStore vectorStore, IChatClient chatClient, string entityPrefix = "recipe")
    {
        _vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        _entityPrefix = entityPrefix ?? "recipe";
    }

    /// <summary>
    /// Get the complete context for an entity by loading ALL its chunks and assembling them.
    /// With multi-chunk strategy, each entity has multiple specialized chunks (Metadata, Description, Ingredients, Instructions).
    /// </summary>
    /// <param name="entityId">The entity ID</param>
    /// <returns>Complete entity text assembled from all chunks</returns>
    public async Task<string> GetEntityContextAsync(int entityId)
    {
        var vectorCollection = _vectorStore.GetCollection<string, SemanticSearchRecord>("RagRecipeEmbeddings");

        // Load all chunks for this entity
        // Chunks follow naming pattern: recipe_5_chunk_0, recipe_5_chunk_1, etc.
        var chunkKeys = new List<string>();
        for (int i = 0; i < 10; i++) // Support up to 10 chunks per entity
        {
            chunkKeys.Add($"{_entityPrefix}_{entityId}_chunk_{i}");
        }

        var chunks = new List<SemanticSearchRecord>();
        await foreach (var record in vectorCollection.GetAsync(chunkKeys))
        {
            if (record is not null)
                chunks.Add(record);
        }

        if (!chunks.Any())
        {
            return string.Empty;
        }

        // Assemble chunks into complete context
        // Order chunks by their type for better readability
        var orderedChunks = chunks
            .OrderBy(c => GetChunkTypeOrder(c.ChunkType))
            .ToList();

        var contextBuilder = new System.Text.StringBuilder();
        foreach (var chunk in orderedChunks)
        {
            contextBuilder.AppendLine($"=== {chunk.ChunkType} ===");
            contextBuilder.AppendLine(chunk.Text);
            contextBuilder.AppendLine();
        }

        return contextBuilder.ToString();
    }

    /// <summary>
    /// Helper to order chunks logically: Metadata → Description → Ingredients → Instructions
    /// </summary>
    private static int GetChunkTypeOrder(string chunkType)
    {
        return chunkType switch
        {
            "Metadata" => 1,
            "Description" => 2,
            "Ingredients" => 3,
            "Instructions" => 4,
            _ => 99
        };
    }

    /// <summary>
    /// Ask a question about a specific entity with conversation history.
    /// Loads the complete entity context once and uses it for the entire conversation.
    /// </summary>
    /// <param name="entityId">The entity to ask about</param>
    /// <param name="question">User's question</param>
    /// <param name="systemPromptTemplate">System prompt template. Use {0} as placeholder for context.</param>
    /// <param name="emptyContextMessage">Message to return when context is not found.</param>
    /// <param name="invalidQuestionMessage">Message to return when question is empty.</param>
    /// <param name="history">Previous messages in the conversation (optional)</param>
    /// <returns>Assistant's response</returns>
    public async Task<string> AskQuestionAsync(
        int entityId,
        string question,
        string systemPromptTemplate,
        string emptyContextMessage = "Je n'ai pas trouvé d'informations sur cet élément.",
        string invalidQuestionMessage = "Je n'ai pas compris votre question. Pouvez-vous la reformuler ?",
        List<ChatMessage>? history = null)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return invalidQuestionMessage;
        }

        // Load the complete entity chunk
        var context = await GetEntityContextAsync(entityId);

        if (string.IsNullOrEmpty(context))
        {
            return emptyContextMessage;
        }

        // Build the conversation messages
        var messages = new List<ChatMessage>();

        // System prompt with complete entity context
        var systemPrompt = string.Format(systemPromptTemplate, context);
        messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

        // Add conversation history if provided
        if (history != null && history.Any())
        {
            messages.AddRange(history);
        }

        // Add current user question
        messages.Add(new ChatMessage(ChatRole.User, question));

        // Get response from LLM
        var response = await _chatClient.GetResponseAsync(messages);

        return response.Text ?? "Désolé, je n'ai pas pu générer une réponse.";
    }
}
