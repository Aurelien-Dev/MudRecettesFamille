using DeepSeek.Core;
using DeepSeek.Core.Models;
using Newtonsoft.Json;
using RecettesFamille.Data;
using RecettesFamille.Data.EntityModel;
using RecettesFamille.Managers.Mappers;
using RecettesFamille.Managers.Models;

namespace RecettesFamille.Managers.AiGenerators;

public class DeepSeekRecipeConverterManager(IConfiguration Config, ApplicationDbContext dbContext) : IRecipeConverteBase
{
    public async Task<RecipeEntity> Convert(string recipe, CancellationToken cancellationToken = default)
    {
        var client = new DeepSeekClient(Config["DEEPSEEK_SECRET"]);

        string newPromptRecetteConvert = dbContext.Prompts.Where(c => c.Name == "GptRecipeConvert").Select(c => c.Prompt).First();
        string ask = $@"Voici une recette à convertir en JSON en respectant les instructions du prompt :

=== Début de la recette ===
{recipe}
=== Fin de la recette ===

Réponds uniquement avec un objet JSON valide, sans texte supplémentaire, sans balises Markdown et sans explication.";


        var request = new ChatRequest
        {
            Messages = [
                Message.NewSystemMessage(newPromptRecetteConvert),
                Message.NewUserMessage(ask)
            ],
            // Specify the model
            Model = Constant.Model.ChatModel
        };

        var chatResponse = await client.ChatAsync(request, cancellationToken);
        if (chatResponse is null)
        {
            Console.WriteLine(client.ErrorMsg);
        }
        string resultText = chatResponse?.Choices.First().Message?.Content;

        await ReportConsumption(chatResponse);

        var serialized = JsonConvert.DeserializeObject<GptRecipe>(resultText);

        return GptMapper.ConvertToRecipeEntity(serialized);
    }


    private async Task ReportConsumption(ChatResponse chatResponse)
    {
        dbContext.AiConsumptions.Add(new AiConsumptionEntity()
        {
            Date = DateTime.UtcNow,
            InputToken = chatResponse.Usage.PromptCacheMissTokens,
            OutputToken = chatResponse.Usage.CompletionTokens,
            InputPrice = 0.27m,
            OutputPrice = 1.10m,
            UseCase = "RecipeConverter",
            AiModelName = "DeepSeek"
        });
        await dbContext.SaveChangesAsync();
    }
}
