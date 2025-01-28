using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Newtonsoft.Json;
using OpenAI.Chat;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Managers.Mappers;

namespace RecettesFamille.Managers.AiGenerators;

public class GptRecipeConverterManager(IConfiguration Config, IDbContextFactory<ApplicationDbContext> contextFactory) : IRecipeConverteBase
{
    private ApplicationDbContext dbContext = null!;

    public async Task<RecipeEntity> Convert(string recipe, CancellationToken cancellationToken = default)
    {
        dbContext = await contextFactory.CreateDbContextAsync(cancellationToken);

        var client = new ChatClient(model: "gpt-4o", apiKey: Config["OPENAI_SECRET"]);

        string newPromptRecetteConvert = dbContext.Prompts.Where(c => c.Name == "GptRecipeConvert").Select(c => c.Prompt).First();
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

        await ReportConsumption(completion);

        var serialized = JsonConvert.DeserializeObject<AiRecipe>(resultText) ?? throw new ApplicationException("Deserialization failed");

        return GptMapper.ConvertToRecipeEntity(serialized);
    }


    private async Task ReportConsumption(ChatCompletion completion)
    {
        dbContext.AiConsumptions.Add(new AiConsumptionEntity()
        {
            Date = DateTime.UtcNow,
            InputToken = completion.Usage.InputTokenCount,
            OutputToken = completion.Usage.OutputTokenCount,
            InputPrice = 2.50m,
            OutputPrice = 10.00m, 
            UseCase = "RecipeConverter",
            AiModelName = "openai-gpt-4o"
        });
        await dbContext.SaveChangesAsync();
    }
}