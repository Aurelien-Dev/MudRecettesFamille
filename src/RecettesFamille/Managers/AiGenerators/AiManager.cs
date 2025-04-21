using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OpenAI.Images;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Data.Repository.Repositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Dto.Models.Blocks;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Managers.Mappers;

namespace RecettesFamille.Managers.AiGenerators;

/// <summary>
/// Manages AI operations for generating images and converting recipes.
/// </summary>
public class AiManager(IServiceProvider serviceProvider, IConfiguration config, IAiRepository aiRepository, IYoutubeRepository youtubeRepository, IRecipeRepository recipeRepository) : IAiManager
{
    /// <summary>
    /// Asks the AI to generate an image based on the recipe name.
    /// </summary>
    /// <param name="recipeName">The name of the recipe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A base64 string representation of the generated image.</returns>
    public async Task<string> AskImage(int recipeId, CancellationToken cancellationToken = default)
    {
        var client = new ImageClient(model: "dall-e-3", apiKey: config["OPENAI_SECRET"]);
        var promptImageGenerator = await aiRepository.GetPrompt("ImageGeneratorPrompt", cancellationToken);

        string rawRecipe = await recipeRepository.GetRawRecipe(recipeId, cancellationToken);

        GeneratedImage image = await client.GenerateImageAsync(string.Format(promptImageGenerator.Prompt, rawRecipe), new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.Standard,
            Size = GeneratedImageSize.W1792xH1024,
            ResponseFormat = GeneratedImageFormat.Bytes,
            Style = GeneratedImageStyle.Vivid
        }, cancellationToken);

        await ReportImageConsumption();

        return $"data:png;base64," + Convert.ToBase64String(image.ImageBytes);
    }

    public async Task<int> AskCalories(List<IngredientDto> ingredients, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default)
    {
        IChatClient client = GetChatClient(aiClientTypeEnum);

        var recipe = ingredients.Select(s => $"{s.Name}:{s.Quantity}").ToList();


        var ask = $@"J'aimerais que tu calcule les calories pour 100 grammes de cette recette :

=== Début de la liste des ingredients ===
{string.Join(Environment.NewLine, recipe)}
=== Fin de la liste des ingredients ===

Réponds uniquement au format json répondant à ce schéma:

{{
    calories: 10
}}";

        var messages = new ChatMessage[]
        {
            new (ChatRole.User, ask)
        };

        var result = await GetChatResponse<AiCalorie>(messages, aiClientTypeEnum);
        return result.Calories;
    }

    /// <summary>
    /// Converts a recipe to a RecipeDto using the specified AI client type.
    /// </summary>
    /// <param name="recipe">The recipe to convert.</param>
    /// <param name="aiClientTypeEnum">The AI client type to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The converted RecipeDto.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid AI client type is provided.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails.</exception>
    public async Task<RecipeDto> ConvertRecipe(string recipe, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default)
    {
        IChatClient client = GetChatClient(aiClientTypeEnum);

        var promptDto = await aiRepository.GetPrompt("RecipeConvertPrompt", cancellationToken);
        var promptRecipeConvert = promptDto.Prompt;

        var ask = $@"Voici une recette à convertir en JSON en respectant les instructions du prompt :

        === Début de la recette ===
        {recipe}
        === Fin de la recette ===

        Réponds uniquement avec un objet JSON valide, sans texte supplémentaire, sans balises Markdown et sans explication.";

        var messages = new ChatMessage[]
        {
            new (ChatRole.System, promptRecipeConvert),
            new (ChatRole.User, ask)
        };

        var result = await GetChatResponse<AiRecipe>(messages, aiClientTypeEnum);

        return GptMapper.ConvertToRecipeDto(result);
    }

    public async Task<string> GetYoutubeResume(YoutubeSummaryJson request, CancellationToken cancellationToken = default)
    {

        IChatClient client = GetChatClient(AiClientType.OpenAi);

        var promptDto = await aiRepository.GetPrompt("YoutubeResume", cancellationToken);

        var ask = $@"
=== Début du transcript ===
{request.Transcript}
=== Fin du transcript ===";

        var messages = new ChatMessage[]
        {
            new (ChatRole.System, promptDto.Prompt),
            new (ChatRole.User, ask)
        };

        var result = await GetChatResponse(messages, AiClientType.OpenAi);

        _ = await youtubeRepository.AddSummary(new YoutubeResumeDto() { Resume = result, Title = request.Title, Url = request.Url });

        return result;
    }


    public async Task<T> GetChatResponse<T>(ChatMessage[] chatMessages, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default)
    {
        var result = await GetChatResponse(chatMessages, aiClientTypeEnum, cancellationToken);
        var serialized = JsonConvert.DeserializeObject<T>(result) ?? throw new InvalidOperationException("Deserialization failed");

        return serialized;
    }

    public async Task<string> GetChatResponse(ChatMessage[] chatMessages, AiClientType aiClientTypeEnum, CancellationToken cancellationToken = default)
    {
        IChatClient client = GetChatClient(aiClientTypeEnum);

        ChatResponse completion = await client.GetResponseAsync(chatMessages, new ChatOptions() { ResponseFormat = ChatResponseFormatJson.Json }, cancellationToken: cancellationToken);

        var resultText = completion.Text;

        ArgumentException.ThrowIfNullOrEmpty(resultText);

        await ReportChatConsumption(completion, aiClientTypeEnum);
        return resultText;
    }


    private IChatClient GetChatClient(AiClientType aiClientTypeEnum)
    {
        IChatClient client;

        switch (aiClientTypeEnum)
        {
            case AiClientType.OpenAi:
                client = serviceProvider.GetRequiredKeyedService<IChatClient>("OpenAi");
                break;
            case AiClientType.DeepSeek:
                client = serviceProvider.GetRequiredKeyedService<IChatClient>("DeepSeek");
                break;
            default:
                throw new ArgumentException("Invalid AI client type", nameof(aiClientTypeEnum));
        }

        return client;
    }

    /// <summary>
    /// Reports the consumption of image generation to the AI repository.
    /// </summary>
    private async Task ReportImageConsumption()
    {
        const string imageModel = "dall-e-3";

        await aiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = 1,
            OutputToken = 0,
            InputPrice = 0.018m,
            OutputPrice = 0,
            UseCase = "ImageCreation",
            AiModelName = $"openai-{imageModel}"
        });
    }

    /// <summary>
    /// Reports the consumption of chat operations to the AI repository.
    /// </summary>
    /// <param name="completion">The chat response completion details.</param>
    /// <exception cref="InvalidOperationException">Thrown when token counts are null.</exception>
    private async Task ReportChatConsumption(ChatResponse completion, AiClientType aiClientTypeEnum)
    {
        if (completion?.Usage?.InputTokenCount == null || completion.Usage.OutputTokenCount == null)
        {
            throw new InvalidOperationException("Token counts are null");
        }

        var (inputPrice, outputPrice, chatModel) = GetPrices(aiClientTypeEnum);

        await aiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = (int)completion.Usage.InputTokenCount,
            OutputToken = (int)completion.Usage.OutputTokenCount,
            InputPrice = inputPrice,
            OutputPrice = outputPrice,
            UseCase = "RecipeConverter",
            AiModelName = $"openai-{chatModel}"
        });
    }

    private static (decimal inputPrice, decimal outputPrice, string chatModel) GetPrices(AiClientType aiClientTypeEnum)
    {
        return aiClientTypeEnum switch
        {
            AiClientType.OpenAi => (2.50m, 10.00m, "gpt-4o"),
            AiClientType.DeepSeek => (0.27m, 1.10m, "deepseek-chat"),
            _ => throw new ArgumentException("Invalid AI client type", nameof(aiClientTypeEnum))
        };
    }
}
