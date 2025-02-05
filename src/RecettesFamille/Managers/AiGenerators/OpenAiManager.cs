﻿using MudBlazor;
using Newtonsoft.Json;
using OpenAI.Chat;
using OpenAI.Images;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Dto.Models;
using RecettesFamille.Managers.AiGenerators.Models;
using RecettesFamille.Managers.Mappers;

namespace RecettesFamille.Managers.AiGenerators;

public class OpenAiManager(IConfiguration Config, IAiRepository AiRepository) : IIaManagerBase
{
    private const string _chatModel = "gpt-4o";
    private const string _imageModel = "dall-e-3";

    public async Task<string> AskImage(string recipeName, CancellationToken cancellationToken = default)
    {
        var client = new ImageClient(model: _imageModel, apiKey: Config["OPENAI_SECRET"]);

        string ask = $@"Un gros plan réaliste de la recette : {recipeName}. 
Dans une ambiance minimaliste, sans accessoires ni distractions en arrière-plan, pour focaliser l'attention sur ses détails. 
L'éclairage est doux et naturel, mettant en avant les textures et les nuances pour une présentation élégante et épurée.";


        GeneratedImage image = await client.GenerateImageAsync(ask, new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1792xH1024,
            ResponseFormat = GeneratedImageFormat.Bytes,
            Style = GeneratedImageStyle.Vivid
        }, cancellationToken);

        await ReportImageConsumption(image);

        return $"data:png;base64," + Convert.ToBase64String(image.ImageBytes);
    }

    public async Task<RecipeDto> ConvertRecipe(string recipe, CancellationToken cancellationToken = default)
    {
        var client = new ChatClient(model: _chatModel, apiKey: Config["OPENAI_SECRET"]);

        PromptDto promptRecetteConvert = await AiRepository.GetPrompt("GptRecipeConvert");

        string newPromptRecetteConvert = promptRecetteConvert.Prompt;
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

        await ReportChatConsumption(completion);

        var serialized = JsonConvert.DeserializeObject<AiRecipe>(resultText) ?? throw new ApplicationException("Deserialization failed");

        return GptMapper.ConvertToRecipeDto(serialized);
    }

    private async Task ReportImageConsumption(GeneratedImage image)
    {
        await AiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = 1,
            OutputToken = 0,
            InputPrice = 0.018m,
            OutputPrice = 0,
            UseCase = "ImageCreation",
            AiModelName = $"openai-{_imageModel}"
        });
    }

    private async Task ReportChatConsumption(ChatCompletion completion)
    {
        await AiRepository.ReportConsumption(new AiConsumptionDto()
        {
            Date = DateTime.UtcNow,
            InputToken = completion.Usage.InputTokenCount,
            OutputToken = completion.Usage.OutputTokenCount,
            InputPrice = 2.50m,
            OutputPrice = 10.00m,
            UseCase = "RecipeConverter",
            AiModelName = $"openai-{_chatModel}"
        });
    }
}