using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Newtonsoft.Json;
using OpenAI.Chat;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Data.EntityModel.RecipeSubEntity;
using RecettesFamille.Managers.Mappers;
using RecettesFamille.Managers.Models;
using System.Reflection;

namespace RecettesFamille.Managers;

public class GptRecipeConverterManager(IConfiguration Config, ApplicationDbContext dbContext)
{
    public async Task<(RecipeEntity, decimal)> Convert(string recipe, CancellationToken cancellationToken = default)
    {
        var client = new ChatClient(model: "gpt-4o", apiKey: Config["OPENAI_SECRET"]);

        string newPromptRecetteConvert = dbContext.Prompts.Where(c => c.Name == "NewPromptRecetteConvert").Select(c => c.Prompt).First();
        string ask = $@"Voici une recette à convertir en JSON en respectant les instructions du prompt :

=== Début de la recette ===
{recipe}
=== Fin de la recette ===

Réponds uniquement avec un objet JSON valide, sans texte supplémentaire, sans balises Markdown et sans explication.";

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(newPromptRecetteConvert),
            new UserChatMessage(ask)
        };

        ChatCompletion completion = await client.CompleteChatAsync(messages, new ChatCompletionOptions() { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() }, cancellationToken: cancellationToken);

        string resultText = completion.Content[0].Text;
        decimal cost = CalculateCost(completion); // À implémenter selon tes besoins

        await ReportConsumption(cost);

        var serialized = JsonConvert.DeserializeObject<GptRecipe>(resultText);

        return (GptMapper.ConvertToRecipeEntity(serialized), cost);
    }



    private async Task ReportConsumption(decimal cost)
    {
        dbContext.GptConsumptions.Add(new GptConsumptionEntity()
        {
            Date = DateTime.UtcNow,
            Cost = cost,
            UseCase = "RecipeConverter"
        });
        await dbContext.SaveChangesAsync();
    }

    private decimal CalculateCost(ChatCompletion result)
    {
        //gpt-4o-mini
        // Tarifs en dollars par token (à ajuster selon les tarifs actuels)
        //decimal costPerInputTokenUsd = 0.150m / 1000000m; // $0.150 / 1M input tokens
        //decimal costPerOutputTokenUsd = 0.600m / 1000000m; // $0.600 / 1M output tokens

        //gpt-4o
        // Tarifs en dollars par token (à ajuster selon les tarifs actuels)
        decimal costPerInputTokenUsd = 2.50m / 1000000m; // $0.150 / 1M input tokens
        decimal costPerOutputTokenUsd = 10.00m / 1000000m; // $0.600 / 1M output tokens

        ////gpt-4o-2024-08-06
        //// Tarifs en dollars par token (à ajuster selon les tarifs actuels)
        //decimal costPerInputTokenUsd = 0.150m / 1000000m; // $0.150 / 1M input tokens
        //decimal costPerOutputTokenUsd = 0.600m / 1000000m; // $0.600 / 1M output tokens

        // Taux de conversion USD -> EUR
        decimal conversionRate = 0.970771m;

        // Obtenir le nombre de tokens utilisés
        int inputTokens = result.Usage.InputTokenCount;
        int outputTokens = result.Usage.OutputTokenCount;

        // Calculer le coût total en dollars
        decimal totalCostUsd = (inputTokens * costPerInputTokenUsd) + (outputTokens * costPerOutputTokenUsd);

        // Convertir le coût en euros
        return totalCostUsd * conversionRate;
    }
}