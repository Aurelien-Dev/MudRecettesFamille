using DeepSeek.Core;
using DeepSeek.Core.Models;
using Newtonsoft.Json;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Managers.Mappers;

namespace RecettesFamille.Managers.AiGenerators;

public class DeepSeekManager(IConfiguration config, IAiRepository aiRepository) : IIaManagerBase
{
    public Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<RecipeDto> ConvertRecipe(string recipe, CancellationToken cancellationToken = default)
    {
        string apiKey = config["DEEPSEEK_SECRET"] ?? throw new InvalidOperationException("Environment variable IsNullOrEmpty (DEEPSEEK_SECRET)");
        var client = new DeepSeekClient(apiKey);


        var promptRecipeConvert = await aiRepository.GetPrompt("GptRecipeConvert", cancellationToken);

        var newPromptRecipeConvert = promptRecipeConvert.Prompt;
        var ask = $@"Voici une recette à convertir en JSON en respectant les instructions du prompt :

=== Début de la recette ===
{recipe}
=== Fin de la recette ===

Réponds uniquement avec un objet JSON valide, sans texte supplémentaire, sans balises Markdown et sans explication.";


        var request = new ChatRequest
        {
            Messages = [
                Message.NewSystemMessage(newPromptRecipeConvert),
                Message.NewUserMessage(ask)
            ],
            // Specify the model
            Model = DeepSeekModels.ChatModel,
            ResponseFormat = new ResponseFormat() { Type = "json_object" }
        };

        var chatResponse = await client.ChatAsync(request, cancellationToken);
        if (chatResponse is null)
        {
            Console.WriteLine(client.ErrorMsg);
            throw new InvalidOperationException("Chat response is null");
        }
        var resultText = chatResponse.Choices.FirstOrDefault()?.Message?.Content ?? throw new InvalidOperationException("Result text is null");

        await ReportConsumption(chatResponse);

        var serialized = JsonConvert.DeserializeObject<AiRecipe>(resultText) ?? throw new InvalidOperationException("Deserialization failed");

        return GptMapper.ConvertToRecipeDto(serialized);
    }


    private async Task ReportConsumption(ChatResponse chatResponse)
    {
        await aiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = chatResponse.Usage?.PromptCacheMissTokens,
            OutputToken = chatResponse.Usage?.CompletionTokens,
            InputPrice = 0.27m,
            OutputPrice = 1.10m,
            UseCase = "RecipeConverter",
            AiModelName = DeepSeekModels.ChatModel
        });
    }
}
