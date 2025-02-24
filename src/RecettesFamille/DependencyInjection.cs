using Microsoft.Extensions.AI;
using OpenAI;
using RecettesFamille.Managers;
using RecettesFamille.Managers.AiGenerators;
using System.ClientModel;

namespace RecettesFamille;

public static class DependencyInjection
{
    public static void AddManagers(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IAiManager, AiManager>();

        services.AddScoped<ErrorManager>();

        var openAiSecret = config["OPENAI_SECRET"];
        var deepSeekSecret = config["DEEPSEEK_SECRET"];

        ArgumentNullException.ThrowIfNull(openAiSecret);
        ArgumentNullException.ThrowIfNull(deepSeekSecret);

        services.AddKeyedChatClient("OpenAi", new OpenAIClient(new ApiKeyCredential(openAiSecret)).AsChatClient("gpt-4o"));
        services.AddKeyedChatClient("DeepSeek", new OpenAIClient(new ApiKeyCredential(deepSeekSecret), new OpenAIClientOptions() { Endpoint = new Uri("https://api.deepseek.com") }).AsChatClient("deepseek-chat"));
    }
}
