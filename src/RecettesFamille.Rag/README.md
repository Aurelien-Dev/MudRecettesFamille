# RecettesFamille.Rag

Bibliothèque RAG (Retrieval-Augmented Generation) générique pour .NET 9. Permet d'ajouter la recherche sémantique et le chat contextuel à n'importe quelle entité via OpenAI embeddings et PostgreSQL/pgvector.

## Installation

**Prérequis :** PostgreSQL 12+ avec extension pgvector

```bash
dotnet add package RecettesFamille.Rag
```

## Configuration

**Program.cs :**

```csharp
using RecettesFamille.Rag.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Enregistrer les services RAG
builder.Services.AddRecetteFamilleRag(options =>
{
    options.ConnectionString = "Host=localhost;Database=mydb;Username=user;Password=pass";
    options.OpenAIKey = "sk-...";
    options.EmbeddingModel = "text-embedding-3-small"; // ou "text-embedding-3-large"
    options.ChatModel = "gpt-4o-mini"; // ou "gpt-4o"
});

// Enregistrer votre DbContext pour les entités
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Ajouter le service de recherche pour votre entité
builder.Services.AddRagSearchService<Recipe>(sp => sp.GetRequiredService<ApplicationDbContext>());

var app = builder.Build();

// Initialiser la base de données RAG
app.Services.InitializeRagDatabase();

app.Run();
```

## Implémentation d'une entité

Votre entité doit implémenter `IIngestionEntity` :

```csharp
using RecettesFamille.Rag.Ingestion;
using System.Security.Cryptography;
using System.Text;

public class Recipe : IIngestionEntity
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Ingredients { get; set; }
    public required string Instructions { get; set; }
    public required string Category { get; set; }

    // Retourne le contenu complet pour recherche
    public string GetSearchableContent()
    {
        return $"{Title}\n{Description}\n{Ingredients}\n{Instructions}";
    }

    // Stratégie multi-chunks : découper en chunks spécialisés pour meilleure pertinence
    public List<SearchableChunk> GetSearchableChunks()
    {
        return new List<SearchableChunk>
        {
            new("Metadata", $"{Title}\nCatégorie: {Category}"),
            new("Description", $"{Title}\n{Description}"),
            new("Ingredients", $"Ingrédients pour {Title}:\n{Ingredients}"),
            new("Instructions", $"Préparation de {Title}:\n{Instructions}")
        };
    }

    public string GetCategory() => Category;

    public string CalculateContentHash()
    {
        var content = $"{Title}|{Description}|{Ingredients}|{Instructions}|{Category}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(hash);
    }
}
```

## Utilisation

### 1. Recherche sémantique globale

```csharp
public class RecipeSearchService
{
    private readonly RagSearchService<Recipe> _ragSearch;

    public RecipeSearchService(RagSearchService<Recipe> ragSearch)
    {
        _ragSearch = ragSearch;
    }

    public async Task<List<Recipe>> SearchAsync(string query)
    {
        return await _ragSearch.SearchByTextAsync(query, maxResults: 10, scoreThreshold: 0.35);
    }
}
```

### 2. Chat contextuel sur une entité

```csharp
public class RecipeChatService
{
    private readonly RagChatService _ragChat;

    public RecipeChatService(RagChatService ragChat)
    {
        _ragChat = ragChat;
    }

    public async Task<string> AskQuestionAsync(int recipeId, string question)
    {
        var systemPrompt = @"Tu es un assistant culinaire. Réponds uniquement avec les informations fournies.

Contexte:
{0}";

        return await _ragChat.AskQuestionAsync(
            recipeId, 
            question, 
            systemPrompt,
            emptyContextMessage: "Recette non trouvée",
            invalidQuestionMessage: "Question vide");
    }
}
```

### 3. Ingestion temps réel (CRUD hooks)

```csharp
public class RecipeService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly RagIngestionService _ragIngestion;

    public RecipeService(ApplicationDbContext dbContext, RagIngestionService ragIngestion)
    {
        _dbContext = dbContext;
        _ragIngestion = ragIngestion;
    }

    public async Task<Recipe> CreateAsync(Recipe recipe)
    {
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        // Hook RAG : ingestion automatique
        await _ragIngestion.IngestSingleAsync(recipe.Id, recipe.GetSearchableContent(), recipe.Category);
        return recipe;
    }

    public async Task<Recipe> UpdateAsync(Recipe recipe)
    {
        _dbContext.Recipes.Update(recipe);
        await _dbContext.SaveChangesAsync();

        // Hook RAG : mise à jour automatique
        await _ragIngestion.UpdateSingleAsync(recipe.Id, recipe.GetSearchableContent(), recipe.Category);
        return recipe;
    }

    public async Task DeleteAsync(int id)
    {
        var recipe = await _dbContext.Recipes.FindAsync(id);
        if (recipe != null)
        {
            _dbContext.Recipes.Remove(recipe);
            await _dbContext.SaveChangesAsync();

            // Hook RAG : suppression automatique
            await _ragIngestion.DeleteSingleAsync(id);
        }
    }
}
```

## API Reference

### RagSearchService\<TEntity\>

| Méthode | Description |
|---------|-------------|
| `Task<List<TEntity>> SearchByTextAsync(string query, int maxResults = 10, double scoreThreshold = 0.35)` | Recherche sémantique avec re-ranking hybride |

### RagChatService

| Méthode | Description |
|---------|-------------|
| `Task<string> GetEntityContextAsync(int entityId)` | Récupère le contexte complet d'une entité (tous les chunks) |
| `Task<string> AskQuestionAsync(int entityId, string question, string systemPromptTemplate, string emptyContextMessage, string invalidQuestionMessage, List<ChatMessage>? history = null)` | Pose une question sur une entité avec historique optionnel |

### RagIngestionService

| Méthode | Description |
|---------|-------------|
| `Task IngestSingleAsync(int entityId, string searchableContent, string category, string entityPrefix = "recipe")` | Ingère une nouvelle entité |
| `Task UpdateSingleAsync(int entityId, string searchableContent, string category, string entityPrefix = "recipe")` | Met à jour une entité existante |
| `Task DeleteSingleAsync(int entityId, string entityPrefix = "recipe")` | Supprime une entité |

### IIngestionEntity (interface)

| Méthode | Description |
|---------|-------------|
| `string GetSearchableContent()` | Retourne le contenu complet pour embedding |
| `List<SearchableChunk> GetSearchableChunks()` | Retourne les chunks spécialisés (stratégie multi-chunks) |
| `string GetCategory()` | Retourne la catégorie pour filtrage |
| `string CalculateContentHash()` | Calcule le hash SHA256 pour détection de changements |

