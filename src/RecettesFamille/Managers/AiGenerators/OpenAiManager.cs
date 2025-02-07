using MudBlazor;
using Newtonsoft.Json;
using OpenAI.Chat;
using OpenAI.Images;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Managers.Mappers;

namespace RecettesFamille.Managers.AiGenerators;

public class OpenAiManager(IConfiguration config, IAiRepository aiRepository) : IIaManagerBase
{
    private const string ChatModel = "gpt-4o";
    private const string ImageModel = "dall-e-3";

    public async Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default)
    {
        var client = new ImageClient(model: ImageModel, apiKey: config["OPENAI_SECRET"]);
        var promptImageGenerator = await aiRepository.GetPrompt("ImageGeneratorPrompt", cancellationToken);

        GeneratedImage image = await client.GenerateImageAsync(string.Format(promptImageGenerator.Prompt, recipeName), new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1792xH1024,
            ResponseFormat = GeneratedImageFormat.Bytes,
            Style = GeneratedImageStyle.Vivid
        }, cancellationToken);

        await ReportImageConsumption();

        return $"data:png;base64," + Convert.ToBase64String(image.ImageBytes);
    }

    public async Task<RecipeDto> ConvertRecipe(string recipe, CancellationToken cancellationToken = default)
    {
        var client = new ChatClient(model: ChatModel, apiKey: config["OPENAI_SECRET"]);
        var promptRecipeConvert = await aiRepository.GetPrompt("RecipeConvertPrompt", cancellationToken);

        var newPromptRecipeConvert = promptRecipeConvert.Prompt;
        var ask = $@"Voici une recette à convertir en JSON en respectant les instructions du prompt :

=== Début de la recette ===
{recipe}
=== Fin de la recette ===

Réponds uniquement avec un objet JSON valide, sans texte supplémentaire, sans balises Markdown et sans explication.";

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(newPromptRecipeConvert),
            new UserChatMessage(ask)
        };

        ChatCompletion completion = await client.CompleteChatAsync(messages, new ChatCompletionOptions() { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() }, cancellationToken: cancellationToken);

        var resultText = completion.Content[0].Text;

        await ReportChatConsumption(completion);

        var serialized = JsonConvert.DeserializeObject<AiRecipe>(resultText) ?? throw new InvalidOperationException("Deserialization failed");

        return GptMapper.ConvertToRecipeDto(serialized);
    }

    private async Task ReportImageConsumption()
    {
        await aiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = 1,
            OutputToken = 0,
            InputPrice = 0.018m,
            OutputPrice = 0,
            UseCase = "ImageCreation",
            AiModelName = $"openai-{ImageModel}"
        });
    }

    private async Task ReportChatConsumption(ChatCompletion completion)
    {
        await aiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = completion.Usage.InputTokenCount,
            OutputToken = completion.Usage.OutputTokenCount,
            InputPrice = 2.50m,
            OutputPrice = 10.00m,
            UseCase = "RecipeConverter",
            AiModelName = $"openai-{ChatModel}"
        });
    }
}