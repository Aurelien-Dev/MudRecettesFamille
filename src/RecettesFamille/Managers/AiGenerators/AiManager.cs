using Microsoft.Extensions.AI;
using Newtonsoft.Json;
using OpenAI.Images;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Managers.Mappers;

namespace RecettesFamille.Managers.AiGenerators;

/// <summary>
/// Manages AI operations for generating images and converting recipes.
/// </summary>
public class AiManager(IServiceProvider serviceProvider, IConfiguration config, IAiRepository aiRepository, IYoutubeRepository youtubeRepository) : IAiManager
{
    /// <summary>
    /// Asks the AI to generate an image based on the recipe name.
    /// </summary>
    /// <param name="recipeName">The name of the recipe.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A base64 string representation of the generated image.</returns>
    public async Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default)
    {
        var client = new ImageClient(model: "dall-e-3", apiKey: config["OPENAI_SECRET"]);
        var promptImageGenerator = await aiRepository.GetPrompt("ImageGeneratorPrompt", cancellationToken);

        GeneratedImage image = await client.GenerateImageAsync(string.Format(promptImageGenerator.Prompt, recipeName), new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.Standard,
            Size = GeneratedImageSize.W1792xH1024,
            ResponseFormat = GeneratedImageFormat.Bytes,
            Style = GeneratedImageStyle.Vivid
        }, cancellationToken);

        await ReportImageConsumption();

        return $"data:png;base64," + Convert.ToBase64String(image.ImageBytes);
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

        ChatResponse completion = await client.GetResponseAsync(messages, new ChatOptions() { ResponseFormat = ChatResponseFormatJson.Json }, cancellationToken: cancellationToken);

        var resultText = completion.Message.Text;

        ArgumentNullException.ThrowIfNullOrEmpty(resultText);

        await ReportChatConsumption(completion, aiClientTypeEnum);

        var serialized = JsonConvert.DeserializeObject<AiRecipe>(resultText) ?? throw new InvalidOperationException("Deserialization failed");

        return GptMapper.ConvertToRecipeDto(serialized);
    }

    public async Task<string> GetYoutubeResume(YoutubeSummaryJson request, CancellationToken cancellationToken = default)
    {
        IChatClient client = serviceProvider.GetRequiredKeyedService<IChatClient>("OpenAi");

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

        ChatResponse completion = await client.GetResponseAsync(messages, new ChatOptions() { ResponseFormat = ChatResponseFormatJson.Text }, cancellationToken: cancellationToken);

        var resultText = completion.Message.Text;

        ArgumentNullException.ThrowIfNullOrEmpty(resultText);

        await ReportChatConsumption(completion, AiClientType.OpenAi);
        _ = await youtubeRepository.AddSummary(new YoutubeSummaryRequestDto() { Resume = resultText, Title = request.Title, Url = request.Url });

        return resultText;
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
