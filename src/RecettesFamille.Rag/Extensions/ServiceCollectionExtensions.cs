using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using OpenAI;
using RecettesFamille.Rag.Configuration;
using RecettesFamille.Rag.Ingestion;
using RecettesFamille.Rag.Search;
using RecettesFamille.Rag.Services;
using System.ClientModel;

namespace RecettesFamille.Rag.Extensions;

/// <summary>
/// Extension methods for registering RecettesFamille RAG services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all RecettesFamille RAG services to the dependency injection container.
    /// Registers: OpenAI clients, vector store, semantic search, ingestion pipeline, and chat services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Configuration action for RagOptions.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRecetteFamilleRag(
        this IServiceCollection services,
        Action<RagOptions> configureOptions)
    {
        var options = new RagOptions
        {
            ConnectionString = string.Empty,
            OpenAIKey = string.Empty
        };
        configureOptions(options);

        // Validate required options
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("ConnectionString is required in RagOptions.");
        }

        if (string.IsNullOrWhiteSpace(options.OpenAIKey))
        {
            throw new InvalidOperationException("OpenAIKey is required in RagOptions.");
        }

        // Register RagOptions as singleton
        services.AddSingleton(options);

        // Configure OpenAI clients
        var openAIClient = new OpenAIClient(new ApiKeyCredential(options.OpenAIKey));

        var chatClient = openAIClient
            .GetChatClient(options.ChatModel)
            .AsIChatClient();

        var embeddingGenerator = openAIClient
            .GetEmbeddingClient(options.EmbeddingModel)
            .AsIEmbeddingGenerator();

        services.AddChatClient(chatClient)
            .UseFunctionInvocation()
            .UseLogging();

        services.AddEmbeddingGenerator(embeddingGenerator);

        // Register vector store (community toolkit handles pgvector internally)
        services.AddPostgresVectorStore(options.ConnectionString);

        // Register core RAG services - inject connection string for workaround
        services.AddSingleton<SemanticSearch>(sp => new SemanticSearch(
            sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>(),
            sp.GetRequiredService<VectorStore>(),
            options.ConnectionString));
        services.AddScoped<DataIngestor>();

        // Register RAG generic services
        services.AddScoped<RagChatService>();
        services.AddScoped<RagIngestionService>(); // ⬅️ Service d'ingestion unitaire temps réel

        // Register ingestion cache DbContext
        services.AddDbContext<IngestionCacheDbContext>(opts =>
            opts.UseNpgsql(options.ConnectionString));

        return services;
    }

    /// <summary>
    /// Adds RecettesFamille RAG search service for a specific entity type.
    /// Must be called after AddRecetteFamilleRag.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to search.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="dbContextFactory">Factory to create the DbContext for the entity.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRagSearchService<TEntity>(
        this IServiceCollection services,
        Func<IServiceProvider, DbContext> dbContextFactory) where TEntity : class
    {
        services.AddScoped(sp =>
        {
            var semanticSearch = sp.GetRequiredService<SemanticSearch>();
            var dbContext = dbContextFactory(sp);
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RagSearchService<TEntity>>>();
            return new RagSearchService<TEntity>(semanticSearch, dbContext, logger);
        });

        return services;
    }

    /// <summary>
    /// Initializes the RAG database contexts (creates/migrates tables).
    /// Should be called at application startup.
    /// </summary>
    /// <param name="services">The service provider.</param>
    public static void InitializeRagDatabase(this IServiceProvider services)
    {
        IngestionCacheDbContext.Initialize(services);
    }
}
