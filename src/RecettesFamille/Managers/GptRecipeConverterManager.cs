using OpenAI.Chat;
using System.Reflection;

namespace RecettesFamille.Managers;

public class GptRecipeConverterManager(IConfiguration Config)
{
    public async Task<(string, decimal)> GenerateDescription()
    {
        var client = new ChatClient(model: "gpt-4o-mini", apiKey: Config["OpenAiSecret"]);
        

        string recetteTest = await ReadEmbeddedResourceAsync("RecetteTest");

        var messages = new ChatMessage[]
        {
        new SystemChatMessage(await ReadEmbeddedResourceAsync("IntroductionPrompt")),
        new SystemChatMessage(await ReadEmbeddedResourceAsync("ResponseAskedPrompt")),
        new UserChatMessage(await ReadEmbeddedResourceAsync("RequestInformationPrompt", (s) => string.Format(s, recetteTest)))
        };

        ChatCompletion completion = await client.CompleteChatAsync(messages);

        string resultText = completion.Content[0].Text;
        decimal cost = CalculateCost(completion); // À implémenter selon tes besoins

        return (resultText, cost);
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
        string assistantPrompt = $"GptRecipeParser.Prompts.{resourceName}.txt";

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
