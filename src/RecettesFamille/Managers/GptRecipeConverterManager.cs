using OpenAI.Chat;
using RecettesFamille.Data.DtoModel;
using System.Reflection;
using System.Text.Json;

namespace RecettesFamille.Managers;

public class GptRecipeConverterManager(IConfiguration Config)
{
    public async Task<(RecetteDto, decimal)> GenerateDescription()
    {
        var client = new ChatClient(model: "gpt-4o", apiKey: Config["OPENAI_SECRET"]);


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

        //deserialise to Rootobject object
        Rootobject obj = JsonSerializer.Deserialize<Rootobject>(resultText);
        

        return (ToRecetteDto(obj), cost);
    }

    public  RecetteDto ToRecetteDto(Rootobject root)
    {
        return new RecetteDto
        {
            Title = root.name,
            Ingredients = root.ingredients.Select(i => new IngredientDto(i.name, IngredientType.Ingredient)
            {
                Quantity = i.quantity,
                Order = 0, // Assurez-vous de mapper correctement l'ordre
                Identifier = string.Empty // Assurez-vous de mapper correctement l'identifiant
            }).ToList(),
            Description = root.description
        };
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



public class Rootobject
{
    public string name { get; set; }
    public string servings { get; set; }
    public Time time { get; set; }
    public Ingredient[] ingredients { get; set; }
    public string description { get; set; }
}

public class Time
{
    public string preparation { get; set; }
    public string cooking { get; set; }
    public string refrigeration { get; set; }
    public string waiting { get; set; }
}

public class Ingredient
{
    public string name { get; set; }
    public string quantity { get; set; }
    public string type { get; set; }
}
