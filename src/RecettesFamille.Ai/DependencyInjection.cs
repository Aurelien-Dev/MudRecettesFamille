
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using OpenAI;
using RecettesFamille.Ai.Services;
using RecettesFamille.Ai.Services.Ingestion;
using System.ClientModel;

namespace RecettesFamille.Data.Repository;

public static class DependencyInjection
{
    public static void AddAi(this IServiceCollection services, IConfigurationManager configuration)
    {
        // You will need to set the endpoint and key to your own values
        // You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
        //   cd this-project-directory
        //   dotnet user-secrets set OpenAI:Key YOUR-API-KEY
        var openAIClient = new OpenAIClient(new ApiKeyCredential(configuration["OPENAI_SECRET"] ?? throw new InvalidOperationException("Missing configuration: OpenAI:Key. See the README for details.")));
        var chatClient = openAIClient.GetChatClient("gpt-4o-mini").AsIChatClient();
        var embeddingGenerator = openAIClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator();

        var vectorStorePath = Path.Combine("/app/data", "vector-store");

        var vectorStore = new JsonVectorStore(vectorStorePath);

        services.AddSingleton<IVectorStore>(vectorStore);
        services.AddScoped<DataIngestor>();
        services.AddSingleton<SemanticSearch>();
        services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
        services.AddEmbeddingGenerator(embeddingGenerator);

        services.AddDbContext<IngestionCacheDbContext>(options => options.UseSqlite("Data Source=/app/data/ingestioncache.db"));
    }
}