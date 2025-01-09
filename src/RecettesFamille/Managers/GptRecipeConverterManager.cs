using Newtonsoft.Json;
using OpenAI.Chat;
using RecettesFamille.Data.EntityModel;
using System.Reflection;

namespace RecettesFamille.Managers;

public class GptRecipeConverterManager(IConfiguration Config)
{
    public async Task<(RecetteEntity, decimal)> Convert(string recipe, CancellationToken cancellationToken = default)
    {
        var client = new ChatClient(model: "gpt-4o", apiKey: Config["OPENAI_SECRET"]);

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(await ReadEmbeddedResourceAsync("IntroductionPrompt")),
            new SystemChatMessage(await ReadEmbeddedResourceAsync("ResponseAskedPrompt")),
            new UserChatMessage(await ReadEmbeddedResourceAsync("RequestInformationPrompt", (s) => string.Format(s, recipe)))
        };

        ChatCompletion completion = await client.CompleteChatAsync(messages, cancellationToken: cancellationToken);

        string resultText = completion.Content[0].Text;
        decimal cost = CalculateCost(completion); // À implémenter selon tes besoins

        JsonSerializerSettings withTypes = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        var serialized = JsonConvert.DeserializeObject<RecetteEntity>(resultText.Replace("```json", string.Empty).Replace("```", string.Empty), withTypes);



        return (serialized, cost);
    }

    private decimal CalculateCost(ChatCompletion result)
    {
        ////gpt-4o-mini
        //// Tarifs en dollars par token (à ajuster selon les tarifs actuels)
        //decimal costPerInputTokenUSD = 0.150m / 1000000m; // $0.150 / 1M input tokens
        //decimal costPerOutputTokenUSD = 0.600m / 1000000m; // $0.600 / 1M output tokens

        ////gpt-4o
        //// Tarifs en dollars par token (à ajuster selon les tarifs actuels)
        //decimal costPerInputTokenUSD = 5m / 1000000m; // $0.150 / 1M input tokens
        //decimal costPerOutputTokenUSD = 15m / 1000000m; // $0.600 / 1M output tokens

        //gpt-4o-2024-08-06
        // Tarifs en dollars par token (à ajuster selon les tarifs actuels)
        decimal costPerInputTokenUsd = 0.150m / 1000000m; // $0.150 / 1M input tokens
        decimal costPerOutputTokenUsd = 0.600m / 1000000m; // $0.600 / 1M output tokens

        // Taux de conversion USD -> EUR
        decimal conversionRate = 0.85m;

        // Obtenir le nombre de tokens utilisés
        int inputTokens = result.Usage.InputTokenCount;
        int outputTokens = result.Usage.OutputTokenCount;

        // Calculer le coût total en dollars
        decimal totalCostUsd = (inputTokens * costPerInputTokenUsd) + (outputTokens * costPerOutputTokenUsd);

        // Convertir le coût en euros
        return totalCostUsd * conversionRate;
    }

    private async Task<string> ReadEmbeddedResourceAsync(string resourceName, Func<string, string>? textFormaterAction = null)
    {
        string assistantPrompt = $"RecettesFamille.Managers.Prompts.{resourceName}.txt";

        var assembly = Assembly.GetExecutingAssembly();
        using (Stream? stream = assembly.GetManifestResourceStream(assistantPrompt))
        {
            if (stream == null)
            {
                throw new FileNotFoundException("Resource not found", assistantPrompt);
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                string text = await reader.ReadToEndAsync();

                if (textFormaterAction != null)
                    return textFormaterAction(text);
                else
                    return text;
            }
        }
    }
}